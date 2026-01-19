using System.Text.Json;
using System.Text.Json.Serialization;
using BauFahrplanMonitor.Core.Helpers;
using BauFahrplanMonitor.Core.Importer.Dto.ZvF;
using BauFahrplanMonitor.Core.Importer.Helper;
using BauFahrplanMonitor.Core.Interfaces;
using BauFahrplanMonitor.Core.Resolver;
using BauFahrplanMonitor.Core.Services;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NLog;
using Npgsql;

namespace BauFahrplanMonitor.Core.Importer.Upsert;

public sealed class ZvFUpserter(
    IDbContextFactory<UjBauDbContext> dbFactory,
    SharedReferenceResolver           resolver,
    ConfigService                     config)
    : ImportUpserterBase(
            config,
            LogManager.GetCurrentClassLogger()),
        IZvFUpserter {
    private ZvFImportStats _stats = null!;

    // =====================================================================
    // ENTRYPOINT mit Progress
    // =====================================================================
    public async Task<UpsertResult> UpsertAsync(
        UjBauDbContext                 db,
        ZvFXmlDocumentDto              dto,
        IProgress<UpsertProgressInfo>? progress,
        CancellationToken              token) {

        _stats = new ZvFImportStats();

        // -------------------------------------------------
        // PHASE A: Referenzen (kurz, sichtbar)
        // -------------------------------------------------
        var vorgangRef = await resolver.ResolveOrCreateVorgangAsync(db, dto.Vorgang, ImportMode.ZvF, token);
        var senderRef  = await resolver.ResolveOrCreateSenderAsync(db, dto.Header, token);

        // 🔑 Sichtbarkeit für andere Threads
        await db.SaveChangesAsync(token);

        // -------------------------------------------------
        // PHASE B: Datei-Inhalte (TX)
        // -------------------------------------------------
        IDbContextTransaction? tx = null;

        try {
            tx = await db.Database.BeginTransactionAsync(token);

            var dokumentRef = await ResolveOrInsertDokumentAsync(
                db, dto, vorgangRef, senderRef, token);

            await UpsertStreckenabschnitteAsync(dto, token, db, dokumentRef);
            await UpsertBbmnAsync(dto, token, db, vorgangRef);

            // -------------------------------------------------
            // ZÜGE + ABWEICHUNGEN + ENTFALLENE (Bulk)
            // -------------------------------------------------
            db.ChangeTracker.AutoDetectChangesEnabled = false;

            // -----------------------------
            // ZÜGE + ABWEICHUNGEN
            // -----------------------------
            var totalZuege = dto.Document.Zuege.Count;
            var index      = 0;

            foreach (var zug in dto.Document.Zuege) {
                token.ThrowIfCancellationRequested();
                index++;

                if (index % 5 == 0 || index == totalZuege) {
                    progress?.Report(new UpsertProgressInfo {
                        Phase   = UpsertPhase.Zuege,
                        Current = index,
                        Total   = totalZuege
                    });
                }

                var zugRef = await ResolveOrInsertZugAsync(db, zug, dokumentRef, token);

                // ✅ Hier bleibt der Parameter korrekt: zug (Dto)
                await UpsertAbweichungenAsync(db, zug, zugRef, token);
            }

            // -----------------------------
            // ENTFALLENE (dedupe + upsert)
            // -----------------------------
            var distinctEntfallen = dto.Document.Entfallen
                .GroupBy(x => new {
                    x.Zugnr,
                    x.Verkehrstag,
                    x.RegelungsartAlt
                })
                .Select(g => g.First())
                .ToList();

            var totalEntfall = distinctEntfallen.Count;
            index = 0;

            foreach (var e in distinctEntfallen) {
                token.ThrowIfCancellationRequested();
                index++;

                if (index % 5 == 0 || index == totalEntfall) {
                    progress?.Report(new UpsertProgressInfo {
                        Phase   = UpsertPhase.Entfallen,
                        Current = index,
                        Total   = totalEntfall
                    });
                }

                await UpsertEntfallenAsync(db, e, dokumentRef, token);
            }

            db.ChangeTracker.AutoDetectChangesEnabled = true;

            await db.SaveChangesAsync(token);
            await tx.CommitAsync(token);
            db.ChangeTracker.Clear();

            return new UpsertResult {
                DokumentRef = dokumentRef,
                ZvFStats    = _stats
            };
        }
        catch (OperationCanceledException) {
            if (tx != null)
                await tx.RollbackAsync(CancellationToken.None);

            throw;
        }
        catch (Exception ex) {
            if (tx == null) throw;
            await tx.RollbackAsync(CancellationToken.None);

            HandleException(ex, "ZvFUpsert", new {
                dto.Document.Dateiname,
                dto.Vorgang.MasterFplo
            });
            throw;
        }
        finally {
            if (tx != null)
                await tx.DisposeAsync();
        }
    }

    // =====================================================================
    // Streckenabschnitte
    // =====================================================================
    private static async Task UpsertStreckenabschnitteAsync(
        ZvFXmlDocumentDto dto,
        CancellationToken token,
        UjBauDbContext    db,
        long              dokumentRef) {

        if (dto.Document.Strecken is not { Count: > 0 })
            return;

        // Alte Einträge für dieses Dokument entfernen
        await db.ZvfDokumentStreckenabschnitte
            .Where(x => x.ZvfDokumentRef == dokumentRef)
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
                    $"Streckenabschnitt ohne Bauzeit (DokRef={dokumentRef}, Start={strecke.StartBst}, Ende={strecke.EndBst})");

            db.ZvfDokumentStreckenabschnitte.Add(
                new ZvfDokumentStreckenabschnitte {
                    ZvfDokumentRef       = dokumentRef,
                    StartBstRl100        = strecke.StartBst      ?? string.Empty,
                    EndBstRl100          = strecke.EndBst        ?? string.Empty,
                    Massnahme            = strecke.Massnahme     ?? string.Empty,
                    Betriebsweise        = strecke.Betriebsweise ?? string.Empty,
                    Grund                = strecke.Grund         ?? string.Empty,
                    Baubeginn            = strecke.Baubeginn.Value,
                    Bauende              = strecke.Bauende.Value,
                    ZeitraumUnterbrochen = strecke.ZeitraumUnterbrochen,
                    VzgStrecke           = strecke.Vzg
                });
        }
    }

    // =====================================================================
    // Bbmn
    // =====================================================================
    private async Task UpsertBbmnAsync(
        ZvFXmlDocumentDto dto,
        CancellationToken token,
        UjBauDbContext    db,
        long              vorgangRef) {

        if (dto.Vorgang.Bbmn is not { Count: > 0 })
            return;

        var bbmns = dto.Vorgang.Bbmn
            .Select(b => b.Trim())
            .Where(b => !string.IsNullOrWhiteSpace(b))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (bbmns.Count == 0)
            return;

        var existing = new HashSet<string>(
            await db.UjbauVorgangBbmn
                .Where(x => x.UjVorgangRef == vorgangRef)
                .Select(x => x.Bbmn)
                .ToListAsync(token),
            StringComparer.OrdinalIgnoreCase);

        foreach (var bbmn in bbmns) {
            // 🔑 globaler Cache (threadsicher)
            if (!resolver.TryRegisterBbmn(vorgangRef, bbmn))
                continue;

            // DB-Duplikate vermeiden
            if (existing.Contains(bbmn))
                continue;

            db.UjbauVorgangBbmn.Add(new UjbauVorgangBbmn {
                UjVorgangRef = vorgangRef,
                Bbmn         = bbmn
            });
        }
    }

    // =====================================================================
    // Exit
    // =====================================================================
    public async Task MarkImportCompletedAsync(long zvfDokumentRef, CancellationToken token) {
        await using var db = await dbFactory.CreateDbContextAsync(token);

        var doc = await db.ZvfDokument.FindAsync([zvfDokumentRef], token);
        if (doc == null)
            return;

        doc.ImportTimestamp = DateTime.Now;

        await db.SaveChangesAsync(token);
    }

    // =====================================================================
    // ABWEICHUNGEN (aggregiert -> einzelne Upserts, FK-sicher)
    // =====================================================================
    private async Task UpsertAbweichungenAsync(
        UjBauDbContext    db,
        ZvFZugDto         zug,
        long              zugRef,
        CancellationToken token) {
        // --------------------------------------------------
        // 0) Alte Abweichungen dieses Zuges löschen
        // --------------------------------------------------
        await DeleteExistingAbweichungenAsync(db, zugRef, token);

        if (zug.Abweichungen.Count == 0)
            return;

        // --------------------------------------------------
        // 1) Fachlich aggregieren (nach Regelungsart)
        // --------------------------------------------------
        var aggregated = zug.Abweichungen
            .GroupBy(a => a.Regelungsart)
            .Select(g => {
                // 🔑 Anchor-RL100 bestimmen (genau 1 erlaubt)
                var anchors = g
                    .Select(x => x.AnchorRl100)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(Ds100Normalizer.Clean)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList();

                var anchor =
                    anchors.Count == 1
                        ? anchors[0]
                        : null;

                return new ZvFZugAbweichung {
                    Zugnummer    = g.First().Zugnummer,
                    Verkehrstag  = g.First().Verkehrstag,
                    Regelungsart = g.Key,
                    AnchorRl100  = anchor,

                    JsonRaw = JsonSerializer.Serialize(
                        g.Select(x =>
                            JsonDocument.Parse(x.JsonRaw).RootElement),
                        new JsonSerializerOptions {
                            DefaultIgnoreCondition =
                                JsonIgnoreCondition.WhenWritingNull
                        })
                };
            })
            .ToList();

        // --------------------------------------------------
        // 2) Persistieren
        // --------------------------------------------------
        foreach (var abw in aggregated) {
            if (string.IsNullOrWhiteSpace(abw.AnchorRl100)) {
                _stats.AbweichungSkippedNoAnchor++;
                continue;
            }

            await InsertAbweichungAsync(db, abw, zugRef, token);
        }

        Logger.Debug(
            "Zug {0}/{1}: {2} Abweichungen → {3} gespeichert, {4} ohne Anchor übersprungen",
            zug.Zugnummer,
            zug.Verkehrstag,
            zug.Abweichungen.Count,
            aggregated.Count - _stats.AbweichungSkippedNoAnchor,
            _stats.AbweichungSkippedNoAnchor);
    }

    private async Task InsertAbweichungAsync(
        UjBauDbContext    db,
        ZvFZugAbweichung  abw,
        long              zugRef,
        CancellationToken token) {
        // --------------------------------------------------
        // Betriebsstelle auflösen / anlegen
        // --------------------------------------------------
        var bstRef = await resolver.ResolveOrCreateBetriebsstelleAsync(
            db,
            abw.AnchorRl100,
            token);

        if (bstRef <= 0) {
            Logger.Error("Abweichung verworfen – Betriebsstelle ungültig: RL100='{0}'",
                abw.AnchorRl100);

            _stats.AbweichungSkippedInvalidBst++;
            bstRef = 0;
        }

        // --------------------------------------------------
        // Insert
        // --------------------------------------------------
        db.ZvfDokumentZugAbweichung.Add(
            new ZvfDokumentZugAbweichung {
                ZvfDokumentZugRef = zugRef,
                Art               = abw.Regelungsart,
                Abweichung        = abw.JsonRaw,
                AbBstRef          = bstRef > 0 ? bstRef : 0
            });

        _stats.AbweichungInserted++;
    }
    
    // =====================================================================
    // Entfallen (idempotent, constraint-sicher)
    // =====================================================================
    private async Task UpsertEntfallenAsync(
        UjBauDbContext     db,
        ZvFZugEntfallenDto e,
        long               dokumentRef,
        CancellationToken  token) {

        var zugnr = (int)e.Zugnr;

        // 🔑 1) Bereits im ChangeTracker?
        var alreadyTracked = db.ChangeTracker
            .Entries<ZvfDokumentZugEntfallen>()
            .Any(x =>
                x.Entity.ZvfDokumentRef == dokumentRef   &&
                x.Entity.Zugnr          == zugnr         &&
                x.Entity.Verkehrstag    == e.Verkehrstag &&
                x.Entity.Art            == e.RegelungsartAlt);

        if (alreadyTracked)
            return;

        // 🔑 2) DB-Zustand bereinigen (UniqueConstraint sicher)
        await db.Database.ExecuteSqlInterpolatedAsync($@"
            DELETE FROM ujbaudb.zvf_dokument_zug_entfallen
            WHERE zvf_dokument_ref = {dokumentRef}
              AND zugnr            = {zugnr}
              AND verkehrstag      = {e.Verkehrstag}
              AND art              = {e.RegelungsartAlt};
            ", token);

        // 🔑 3) Neu hinzufügen
        db.ZvfDokumentZugEntfallen.Add(new ZvfDokumentZugEntfallen {
            ZvfDokumentRef = dokumentRef,
            Zugnr          = zugnr,
            Zugbez         = e.Zugbez,
            Verkehrstag    = e.Verkehrstag,
            Art            = e.RegelungsartAlt
        });

        _stats.EntfallenInserted++;
    }

    // =====================================================================
    // Document
    // =====================================================================
    private async Task<long> ResolveOrInsertDokumentAsync(
        UjBauDbContext    db,
        ZvFXmlDocumentDto dto,
        long              vorgangRef,
        long?             senderRef,
        CancellationToken token) {

        // Fast Lookup (read-only)
        var existing = await db.ZvfDokument
            .AsNoTracking()
            .FirstOrDefaultAsync(d =>
                    d.UjbauVorgangRef == vorgangRef &&
                    d.Dateiname       == dto.Document.Dateiname,
                token);

        if (existing != null)
            return existing.Id;

        // Region auflösen
        if (string.IsNullOrWhiteSpace(dto.Document.MasterRegion))
            throw new InvalidOperationException("Masterniederlassung fehlt");

        var regionRef = await resolver.ResolveRegionAsync(db, dto.Document.MasterRegion, token);

        if (regionRef <= 0)
            throw new InvalidOperationException($"Region '{dto.Document.MasterRegion}' konnte nicht aufgelöst werden");

        // Create
        var doc = new ZvfDokument {
            UjbauVorgangRef = vorgangRef,
            SenderRef       = senderRef ?? throw new InvalidOperationException("SenderRef fehlt"),
            ExportTimestamp = dto.Header.Timestamp,

            VersionMajor = dto.Document.Version.Major,
            VersionMinor = dto.Document.Version.Minor,
            VersionSub   = dto.Document.Version.Sub,
            Version      = dto.Document.Version.VersionNumeric ?? 0,

            RegionRef   = regionRef,
            Endstueck   = dto.Document.Endstueck,
            AntwortBis  = dto.Document.AntwortBis,
            BaudatumVon = dto.Document.BauDatumVon,
            BaudatumBis = dto.Document.BauDatumBis,
            Allgemein   = dto.Document.AllgemeinText,
            Dateiname   = dto.Document.Dateiname,
        };

        db.ZvfDokument.Add(doc);

        try {
            await db.SaveChangesAsync(token);
            return doc.Id;
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex)) {
            var winnerId = await db.ZvfDokument
                .Where(d =>
                    d.UjbauVorgangRef == vorgangRef &&
                    d.Dateiname       == dto.Header.FileName)
                .Select(d => d.Id)
                .SingleOrDefaultAsync(token);

            if (winnerId > 0)
                return winnerId;

            throw new InvalidOperationException(
                "ZvfDokument konnte nach UniqueViolation nicht erneut gelesen werden", ex);
        }
    }

    // =====================================================================
    // Züge
    // =====================================================================
    private async Task<long> ResolveOrInsertZugAsync(
        UjBauDbContext    db,
        ZvFZugDto         zug,
        long              dokumentRef,
        CancellationToken token) {

        // Exists?
        var existing = await db.ZvfDokumentZug
            .AsTracking()
            .FirstOrDefaultAsync(z =>
                    z.ZvfDokumentRef == dokumentRef   &&
                    z.Zugnr          == zug.Zugnummer &&
                    z.Verkehrstag    == zug.Verkehrstag,
                token);

        // Referenzen auflösen (FKs sind NOT NULL)
        var kundeRef = await resolver.ResolveOrCreateKundeAsync(db, zug.Betreiber, token);

        var abgangBstRef = await resolver.ResolveOrCreateBetriebsstelleSmartAsync(
            db,
            zug.Regelweg?.Abgangsbahnhof?.Ds100,
            zug.Regelweg?.Abgangsbahnhof?.Value,
            "ResolveOrInsertZugAsync",
            zug.Zugnummer,
            zug.Verkehrstag,
            token);

        var zielBstRef = await resolver.ResolveOrCreateBetriebsstelleSmartAsync(
            db,
            zug.Regelweg?.Zielbahnhof?.Ds100,
            zug.Regelweg?.Zielbahnhof?.Value,
            "ResolveOrInsertZugAsync",
            zug.Zugnummer,
            zug.Verkehrstag,
            token);

        if (abgangBstRef < 0 || zielBstRef < 0) {
            Logger.Warn("Ungültige Betriebsstellen-Refs ignoriert: Abgang={0}, Ziel={1}, Zug={2}/{3}",
                abgangBstRef, zielBstRef, zug.Zugnummer, zug.Verkehrstag);
            throw (new Exception("Abbruch"));
        }

        // INSERT
        if (existing == null) {
            var entity = new ZvfDokumentZug {
                ZvfDokumentRef = dokumentRef,
                Verkehrstag    = zug.Verkehrstag,
                Zugnr          = zug.Zugnummer,
                Zugbez         = zug.Zugbez,

                KundeRef             = kundeRef,
                RegelwegAbgangBstRef = abgangBstRef,
                RegelwegZielBstRef   = zielBstRef,

                Aenderung     = zug.Aenderung,
                RegelwegLinie = zug.Regelweg?.LinienNr ?? string.Empty,
                Klv           = zug.Klv,
                Skl           = zug.Skl,
                Bza           = zug.Bza,
                Bemerkung     = zug.Bemerkungen,
                Bedarf        = zug.Bedarf,
                Sonderzug     = zug.Sonder,
            };

            _stats.ZuegeInserted++;

            db.ZvfDokumentZug.Add(entity);
            await db.SaveChangesAsync(token); // (wie bei dir; kann man später noch optimieren)

            return entity.Id;
        }

        // UPDATE (nur bei echten Änderungen)
        SetIfDifferent(existing.Zugbez, zug.Zugbez, v => existing.Zugbez                                           = v);
        SetIfDifferent(existing.Aenderung, zug.Aenderung, v => existing.Aenderung                                  = v);
        SetIfDifferent(existing.RegelwegLinie, zug.Regelweg?.LinienNr ?? string.Empty, v => existing.RegelwegLinie = v);
        SetIfDifferent(existing.Klv, zug.Klv, v => existing.Klv                                                    = v);
        SetIfDifferent(existing.Skl, zug.Skl, v => existing.Skl                                                    = v);
        SetIfDifferent(existing.Bza, zug.Bza, v => existing.Bza                                                    = v);
        SetIfDifferent(existing.Bemerkung, zug.Bemerkungen, v => existing.Bemerkung                                = v);
        SetIfDifferent(existing.Bedarf, zug.Bedarf, v => existing.Bedarf                                           = v);
        SetIfDifferent(existing.Sonderzug, zug.Sonder, v => existing.Sonderzug                                     = v);

        // FK-Refs
        SetIfDifferent(existing.KundeRef, kundeRef, v => existing.KundeRef                             = v);
        SetIfDifferent(existing.RegelwegAbgangBstRef, abgangBstRef, v => existing.RegelwegAbgangBstRef = v);
        SetIfDifferent(existing.RegelwegZielBstRef, zielBstRef, v => existing.RegelwegZielBstRef       = v);

        _stats.ZuegeUpdated++;
        return existing.Id;
    }

    // =====================================================================
    // Helper
    // =====================================================================
    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

    private static void SetIfDifferent<T>(T current, T next, Action<T> setter) {
        if (EqualityComparer<T>.Default.Equals(current, next))
            return;

        setter(next);
    }

    private static async Task DeleteExistingAbweichungenAsync(
        UjBauDbContext    db,
        long              zugRef,
        CancellationToken token) {
        await db.ZvfDokumentZugAbweichung
            .Where(x => x.ZvfDokumentZugRef == zugRef)
            .ExecuteDeleteAsync(token);
    }
}