using System.Diagnostics;
using System.Text.Json;
using BauFahrplanMonitor.Core.Helpers;
using BauFahrplanMonitor.Core.Importer.Dto.Fplo;
using BauFahrplanMonitor.Core.Importer.Helper;
using BauFahrplanMonitor.Core.Importer.Mapper;
using BauFahrplanMonitor.Core.Interfaces;
using BauFahrplanMonitor.Core.Resolver;
using BauFahrplanMonitor.Core.Services;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Models;
using Microsoft.EntityFrameworkCore;
using NLog;
using Npgsql;

namespace BauFahrplanMonitor.Core.Importer.Upsert;

/// <summary>
/// Upserter für ÜB-Dokumente
/// vollständig, eigenständig, FK-sicher
/// </summary>
public sealed class FploUpserter(
    IDbContextFactory<UjBauDbContext> dbFactory,
    SharedReferenceResolver           resolver,
    ConfigService                     config)
    : ImportUpserterBase(config, LogManager.GetCurrentClassLogger()), IFploUpserter {

    private FploImportStats Stats { get; set; } = new();

    // =====================================================================
    // ENTRYPOINT
    // =====================================================================
    public async Task<UpsertResult> UpsertAsync(
        UjBauDbContext                db,
        FploXmlDocumentDto            dto,
        IProgress<UpsertProgressInfo> progress,
        CancellationToken             token) {
        token.ThrowIfCancellationRequested();

        Stats = new FploImportStats {
            Dokumente = 1
        };
        var sw = Stopwatch.StartNew();

        DebugTraceHelper.TraceDocumentRegions(Logger, "Upsert.Entry", dto);

        using (ScopeContext.PushProperty("ImportFile", dto.Document.Dateiname)) {
            // -------------------------------------------------
            // PHASE A: Referenzen
            // -------------------------------------------------
            var vorgangRef = await resolver.ResolveOrCreateVorgangAsync(db, dto.Vorgang, ImportMode.Fplo, token);
            var senderRef  = await resolver.ResolveOrCreateSenderAsync(db, dto.Header, token);

            await db.SaveChangesAsync(token);

            // -------------------------------------------------
            // PHASE B: Transaktion
            // -------------------------------------------------
            await using var tx = await db.Database.BeginTransactionAsync(token);

            try {
                DebugTraceHelper.TraceDocumentRegions(
                    Logger,
                    "Before.ResolveDokument",
                    dto);

                var dokumentRef = await ResolveOrInsertDokumentAsync(db, dto, vorgangRef, senderRef, token);
                await UpsertStreckenabschnitteAsync(dto, token, db, dokumentRef);
                var factoryResult = FploZugFactory.Build(dto.Document);

                Stats.SevsGelesen += factoryResult.SevsGelesen;

                var total = factoryResult.Zuege.Count;
                var index = 0;

                foreach (var zug in factoryResult.Zuege) {
                    token.ThrowIfCancellationRequested();
                    index++;

                    if (zug.Zugnummer <= 0 || zug.Verkehrstag == default) {
                        Logger.Warn(
                            "Ungültiger Zug übersprungen (Zugnr={0}, Verkehrstag={1})",
                            zug.Zugnummer,
                            zug.Verkehrstag);
                        continue;
                    }

                    progress?.Report(new UpsertProgressInfo {
                        Phase   = UpsertPhase.Zuege,
                        Current = index,
                        Total   = total
                    });

                    using (ScopeContext.PushProperty("Zugnr", zug.Zugnummer))
                    using (ScopeContext.PushProperty("Verkehrstag", zug.Verkehrstag.ToString("yyyy-MM-dd"))) {
                        try {
                            var zugRef = await ResolveOrInsertZugAsync(db, zug, dokumentRef, dto, token);

                            // ✅ Fahrplan
                            if (zug.Fahrplan is { Count: > 0 }) {
                                await UpsertFahrplanAsync(db, zugRef, zug.Fahrplan, token);
                            }

                            // ✅ Regelungen
                            var key = new FploZugKey(zug.Zugnummer, zug.Verkehrstag);
                            if (factoryResult.Regelungen.TryGetValue(key, out var regs)) {
                                foreach (var reg in regs) {
                                    await UpsertRegelungAsync(db, reg, zugRef, token);
                                }
                            }
                        }
                        catch (Exception ex) {
                            HandleException(ex, "FploUpsert", new {
                                dto.Document.Dateiname,
                                dto.Vorgang.MasterFplo
                            });
                        }
                    }
                }

                await db.SaveChangesAsync(token);
                await tx.CommitAsync(token);

                sw.Stop();
                Logger.Info(
                    $"Fplo-Import abgeschlossen: Datei={dto.Document.Dateiname}, Dauer={sw.Elapsed}, Stats={Stats}");

                return new UpsertResult {
                    DokumentRef = dokumentRef,
                    FploStats   = Stats
                };
            }
            catch (Exception ex) {
                await tx.RollbackAsync(CancellationToken.None);

                HandleException(ex, "FploUpsert", new {
                    dto.Document.Dateiname,
                    dto.Vorgang.MasterFplo
                });

                throw;
            }
        }
    }

    // =====================================================================
    // Fahrplan
    // =====================================================================
    private async Task UpsertFahrplanAsync(
        UjBauDbContext                  db,
        long                            zugRef,
        IEnumerable<FploZugFahrplanDto> fahrplan,
        CancellationToken               token) {
        // 1️⃣ Alte Fahrplaneinträge für diesen Zug löschen
        await db.FploDokumentZugFahrplan
            .Where(f => f.FploDokumentZugRef == zugRef)
            .ExecuteDeleteAsync(token);

        // 2️⃣ Neu einfügen
        var lfdNr = 1;
        foreach (var fp in fahrplan.OrderBy(f => f.LfdNr)) {
            long? bstRef = null;
            if (!string.IsNullOrWhiteSpace(fp.BstDs100)) {
                bstRef = await resolver.ResolveOrCreateBetriebsstelleAsync(
                    db, fp.BstDs100, token);
            }

            if (bstRef < 0) {
                Logger.Warn("Ungültige Betriebsstellen-Refs ignoriert: Ds100={0}", bstRef);
                throw (new Exception("Abbruch"));
            }
            
            db.FploDokumentZugFahrplan.Add(
                new FploDokumentZugFahrplan {
                    FploDokumentZugRef = zugRef,
                    Lfdnr              = lfdNr++,
                    BstRef             = bstRef,
                    Ankunft            = fp.AnkunftsZeit,
                    Abfahrt            = fp.AbfahrtsZeit,
                    Haltart            = fp.HalteArt,
                    Bemerkung          = fp.Bemerkung,
                    Bfpl = fp.EbulaFahrplanHeft != null
                        ? JsonSerializer.Serialize(new {
                            fp.EbulaVglZug,
                            fp.EbulaVglMbr,
                            fp.EbulaVglBrs,
                            fp.EbulaFahrplanHeft,
                            fp.EbulaFahrplanSeite
                        })
                        : null
                });
        }
    }

    // =====================================================================
    // Exit
    // =====================================================================
    public async Task MarkImportCompletedAsync(long dokumentRef, CancellationToken token) {
        await using var db = await dbFactory.CreateDbContextAsync(token);

        var doc = await db.FploDokument.FindAsync([dokumentRef], token);
        if (doc == null)
            return;

        doc.ImportTimestamp = DateTime.Now;

        await db.SaveChangesAsync(token);
    }

    // =====================================================================
    // ZUG
    // =====================================================================
    private async Task<long> ResolveOrInsertZugAsync(
        UjBauDbContext     db,
        FploZugDto         zug,
        long               dokumentRef,
        FploXmlDocumentDto dto,
        CancellationToken  token) {
        var existing =
            await db.FploDokumentZug.FirstOrDefaultAsync(
                z =>
                    z.FploDokumentRef == dokumentRef   &&
                    z.Zugnr           == zug.Zugnummer &&
                    z.Verkehrstag     == zug.Verkehrstag,
                token);

        var kundeRef = await ResolveKundeAsync(db, zug, dto, token);
        var regelwegAbBstRef =
            await resolver.ResolveOrCreateBetriebsstelleAsync(
                db,
                zug.Regelweg?.Abgangsbahnhof?.Ds100,
                token);

        var regelwegZielBstRef =
            await resolver.ResolveOrCreateBetriebsstelleAsync(
                db,
                zug.Regelweg?.Zielbahnhof?.Ds100,
                token);

        if (regelwegAbBstRef < 0 || regelwegZielBstRef < 0) {
            Logger.Warn("Ungültige Betriebsstellen-Refs ignoriert: Abgang={0}, Ziel={1}, Zug={2}/{3}",
                regelwegAbBstRef, regelwegZielBstRef, zug.Zugnummer, zug.Verkehrstag);
            throw (new Exception("Abbruch"));
        }

        // Update
        if (existing != null) {
            existing.KundeRef            = kundeRef;
            existing.Bedarf              = zug.Bedarf;
            existing.KundeRef            = kundeRef;
            existing.Zugbez              = zug.Zugbez;
            existing.Zuggat              = zug.ZugGattung.ToString();
            existing.Sicherheitsrelevant = zug.IstSicherheitsrelevant;
            existing.Lauterzug           = zug.LauterZug;
            existing.Vmax                = zug.Vmax;
            existing.Tfz                 = zug.Tfzf;
            existing.Last                = zug.Last;
            existing.Laenge              = zug.Laenge;
            existing.Brems               = zug.Bremssystem;
            existing.Ebula               = zug.Ebula;
            existing.Skl                 = zug.Skl;
            existing.Klv                 = zug.Klv;
            existing.Bemerkung           = zug.Bemerkungen;
            existing.RegelwegLinie       = zug.Regelweg?.LinienNr ?? string.Empty;
            existing.RegelwegAbBstRef    = regelwegAbBstRef;
            existing.RegelwegZielBstRef  = regelwegZielBstRef;
            Stats.ZuegeUpdated++;
            return existing.Id;
        }

        // Insert
        var entity = new FploDokumentZug {
            FploDokumentRef     = dokumentRef,
            Zugnr               = zug.Zugnummer,
            Verkehrstag         = zug.Verkehrstag,
            Bedarf              = zug.Bedarf,
            KundeRef            = kundeRef,
            Zugbez              = zug.Zugbez,
            Zuggat              = zug.ZugGattung.ToString(),
            Sicherheitsrelevant = zug.IstSicherheitsrelevant,
            Lauterzug           = zug.LauterZug,
            Vmax                = zug.Vmax,
            Tfz                 = zug.Tfzf,
            Last                = zug.Last,
            Laenge              = zug.Laenge,
            Brems               = zug.Bremssystem,
            Ebula               = zug.Ebula,
            Skl                 = zug.Skl,
            Klv                 = zug.Klv,
            Bemerkung           = zug.Bemerkungen,
            RegelwegLinie       = zug.Regelweg?.LinienNr ?? string.Empty,
            RegelwegAbBstRef    = regelwegAbBstRef,
            RegelwegZielBstRef  = regelwegZielBstRef
        };

        db.FploDokumentZug.Add(entity);
        await db.SaveChangesAsync(token);

        Stats.ZuegeInserted++;
        return entity.Id;
    }

    // =====================================================================
    // KUNDE
    // =====================================================================
    private async Task<long> ResolveKundeAsync(
        UjBauDbContext     db,
        FploZugDto         zug,
        FploXmlDocumentDto dto,
        CancellationToken  token) {
        if (!string.IsNullOrWhiteSpace(zug.Betreiber))
            return await resolver.ResolveOrCreateKundeAsync(
                db, zug.Betreiber, token);

        var empfaenger = dto.Header.Empfaenger;
        if (empfaenger is not { Count: 1 })
            return 0;
        var kbez = empfaenger[0];
        var id = await db.BasisKunde
            .Where(k => k.Kbez == kbez)
            .Select(k => k.Id)
            .OrderBy(x => x)
            .FirstOrDefaultAsync(token);

        return id > 0 ? id : 0;

    }

    // =====================================================================
    // Document
    // =====================================================================
    private async Task<long> ResolveOrInsertDokumentAsync(
        UjBauDbContext     db,
        FploXmlDocumentDto dto,
        long               vorgangRef,
        long?              senderRef,
        CancellationToken  token) {
        Logger.Debug(
            "[TRACE:ResolveDokument.Entry] Datei='{0}', Region='{1}', Masterniederlassung='{2}'",
            dto.Document.Dateiname,
            dto.Document.Region,
            dto.Document.MasterRegion);

        // -------------------------------------------------
        // Fast Lookup (read-only)
        // -------------------------------------------------
        var existing = await db.FploDokument
            .AsNoTracking()
            .FirstOrDefaultAsync(d =>
                    d.UjbauVorgangRef == vorgangRef &&
                    d.Dateiname       == dto.Document.Dateiname,
                token);

        if (existing != null)
            return existing.Id;

        // -------------------------------------------------
        // Region auflösen
        // -------------------------------------------------
        if (string.IsNullOrWhiteSpace(dto.Document.MasterRegion))
            throw new InvalidOperationException("Masterniederlassung fehlt");

        var masterRegionRef = await resolver.ResolveRegionAsync(db, dto.Document.MasterRegion, token);
        if (masterRegionRef <= 0)
            throw new InvalidOperationException($"MasterRegion '{dto.Document.MasterRegion}' konnte nicht aufgelöst werden");

        if (string.IsNullOrWhiteSpace(dto.Document.Region))
            throw new InvalidOperationException("Region fehlt");
        var regionRef = await resolver.ResolveRegionAsync(db, dto.Document.Region, token);
        if (regionRef <= 0)
            throw new InvalidOperationException($"Region '{dto.Document.Region}' konnte nicht aufgelöst werden");

        Logger.Debug(
            "[TRACE:ResolveDokument.Resolved] Datei='{0}', RegionRef={1}, MasterRegionRef={2}",
            dto.Document.Dateiname,
            regionRef,
            masterRegionRef);

        // -------------------------------------------------
        // Create
        // -------------------------------------------------
        var doc = new FploDokument() {
            UjbauVorgangRef = vorgangRef,
            SenderRef       = senderRef ?? throw new InvalidOperationException("SenderRef fehlt"),
            ExportTimestamp = dto.Header.Timestamp,

            VersionMajor     = dto.Document.Version.Major,
            VersionMinor     = dto.Document.Version.Minor,
            VersionSub       = dto.Document.Version.Sub,
            Version          = dto.Document.Version.VersionNumeric ?? 0,
            RegionRef        = regionRef,
            MasterRegionRef  = masterRegionRef,
            GueltigkeitVon   = dto.Document.GueltigAb,
            GueltigkeitBis   = dto.Document.GueltigBis,
            Allgemein        = dto.Document.AllgemeinText,
            Dateiname        = dto.Document.Dateiname,
            IstEntwurf       = dto.Document.IstEntwurf,
            IstTeillieferung = dto.Document.IstTeillieferung,
            IstNachtrag      = dto.Document.IstNachtrag
        };

        db.FploDokument.Add(doc);

        try {
            await db.SaveChangesAsync(token);
            return doc.Id;
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex)) {
            var winnerId = await db.FploDokument
                .Where(d =>
                    d.UjbauVorgangRef == vorgangRef &&
                    d.Dateiname       == dto.Header.FileName)
                .Select(d => d.Id)
                .SingleOrDefaultAsync(token);

            if (winnerId > 0)
                return winnerId;

            throw new InvalidOperationException("FploDokument konnte nach UniqueViolation nicht erneut gelesen werden", ex);
        }
    }

    // =====================================================================
    // STRECKEN
    // =====================================================================
    private static async Task UpsertStreckenabschnitteAsync(FploXmlDocumentDto dto, CancellationToken token,
        UjBauDbContext                                                         db,  long              dokumentRef) {
        if (dto.Document.Strecken is { Count: > 0 }) {
            // Alte Einträge für dieses Dokument entfernen
            // (Dokument ist die einzige Identität)
            await db.FploDokumentStreckenabschnitte
                .Where(x => x.FploDokumentRef == dokumentRef)
                .ExecuteDeleteAsync(token);

            var uniqueStrecken = dto.Document.Strecken
                .GroupBy(s => new {
                    s.StartBst,
                    s.EndBst,
                    s.Massnahme,
                    s.Betriebsweise,
                    s.Grund,
                    s.Baubeginn,
                    s.Bauende
                })
                .Select(g => g.First())
                .ToList();

            foreach (var strecke in uniqueStrecken) {
                token.ThrowIfCancellationRequested();

                if (strecke.Baubeginn == null || strecke.Bauende == null)
                    throw new InvalidOperationException(
                        $"Streckenabschnitt ohne Baubeginn/Bauende (DokRef={dokumentRef})");

                db.FploDokumentStreckenabschnitte.Add(
                    new FploDokumentStreckenabschnitte {
                        FploDokumentRef = dokumentRef,
                        StartBstRl100   = strecke.StartBst      ?? string.Empty,
                        EndBstRl100     = strecke.EndBst        ?? string.Empty,
                        Massnahme       = strecke.Massnahme     ?? string.Empty,
                        Betriebsweise   = strecke.Betriebsweise ?? string.Empty,
                        Grund           = strecke.Grund         ?? string.Empty,
                        Baubeginn       = strecke.Baubeginn.Value,
                        Bauende         = strecke.Bauende.Value
                    });
            }
        }
    }

    // =====================================================================
    // REGELUNG (DETAIL)
    // =====================================================================
    private async Task UpsertRegelungAsync(
        UjBauDbContext    db,
        FploRegelungDto   regelung,
        long              zugRef,
        CancellationToken token) {
        var ankerBstRef =
            await resolver.ResolveOrCreateBetriebsstelleAsync(
                db, regelung.AnchorRl100!, token);

        var entity = await db.FploDokumentZugRegelung
            .SingleOrDefaultAsync(
                r =>
                    r.FploDokumentZugRef == zugRef       &&
                    r.Art                == regelung.Art &&
                    r.AnkerBstRef        == ankerBstRef,
                token);

        if (entity != null) {
            // 🔁 MERGE
            entity.Regelung ??= regelung.JsonRaw;

            return;
        }

        db.FploDokumentZugRegelung.Add(
            new FploDokumentZugRegelung {
                FploDokumentZugRef = zugRef,
                Art                = regelung.Art,
                AnkerBstRef        = ankerBstRef,
                Regelung           = regelung.JsonRaw
            });
        
        
    }

    // =====================================================================
    // Helper
    // =====================================================================
    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}