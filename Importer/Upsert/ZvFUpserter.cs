using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NLog;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Helpers;
using BauFahrplanMonitor.Importer.Dto.ZvF;
using BauFahrplanMonitor.Importer.Helper;
using BauFahrplanMonitor.Interfaces;
using BauFahrplanMonitor.Models;
using BauFahrplanMonitor.Resolver;
using BauFahrplanMonitor.Services;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace BauFahrplanMonitor.Importer.Upsert;

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
            // ZÜGE + ENTFALLENE (Bulk)
            // -------------------------------------------------
            db.ChangeTracker.AutoDetectChangesEnabled = false;

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
                await UpsertAbweichungenAsync(db, zug, zugRef, token);
            }

            // ENTFALLENE
            var totalEntfall = dto.Document.Entfallen.Count;
            index = 0;

            foreach (var e in dto.Document.Entfallen) {
                token.ThrowIfCancellationRequested();
                index++;

                if (index % 5 == 0 || index == totalEntfall) {
                    progress?.Report(new UpsertProgressInfo {
                        Phase   = UpsertPhase.Entfallen,
                        Current = index,
                        Total   = totalEntfall
                    });
                }

                await UpsertEntfallenAsync(db, e, dokumentRef);
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
    private static async Task UpsertStreckenabschnitteAsync(ZvFXmlDocumentDto dto, CancellationToken token,
        UjBauDbContext                                                        db,  long              dokumentRef) {
        if (dto.Document.Strecken is { Count: > 0 }) {
            // Alte Einträge für dieses Dokument entfernen
            // (Dokument ist die einzige Identität)
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
                        $"Streckenabschnitt ohne Bauzeit (DokRef={dokumentRef}, " +
                        $"Start={strecke.StartBst}, Ende={strecke.EndBst})");

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
    }

    // =====================================================================
    // Bbmn
    // =====================================================================
    private async Task UpsertBbmnAsync(ZvFXmlDocumentDto dto, CancellationToken token, UjBauDbContext db,
        long                                             vorgangRef) {
        if (dto.Vorgang.Bbmn is { Count: > 0 }) {
            var bbmns = dto.Vorgang.Bbmn
                .Select(b => b.Trim())
                .Where(b => !string.IsNullOrWhiteSpace(b))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (bbmns.Count > 0) {
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
    // ABWEICHUNGEN
    // =====================================================================
    private async Task UpsertAbweichungenAsync(
        UjBauDbContext    db,
        ZvFZugDto         zug,
        long              zugRef,
        CancellationToken token) {
        var grouped = zug.Abweichungen
            .GroupBy(a => a.Regelungsart)
            .Select(g => new ZvFZugAbweichung {
                Zugnummer    = g.First().Zugnummer,
                Verkehrstag  = g.First().Verkehrstag,
                Regelungsart = g.Key,

                // 🔑 fachliche Aggregation
                JsonRaw = JsonSerializer.Serialize(
                    g.Select(x => JsonDocument.Parse(x.JsonRaw).RootElement),
                    new JsonSerializerOptions {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    }),

                // Anchor: erster sinnvoller (oder null)
                AnchorRl100 = g
                    .Select(x => x.AnchorRl100)
                    .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
            })
            .ToList();

        foreach (var abw in grouped) {
            await UpsertAbweichungAsync(db, abw, zugRef, token);
        }

        Logger.Debug(
            "Zug {0}/{1}: {2} Abweichungen → {3} aggregiert",
            zug.Zugnummer,
            zug.Verkehrstag,
            zug.Abweichungen.Count,
            grouped.Count);
    }

    // =====================================================================
    // Entfallen 
    // =====================================================================
    private Task UpsertEntfallenAsync(
        UjBauDbContext     db,
        ZvFZugEntfallenDto e,
        long               dokumentRef) {
        var entity = new ZvfDokumentZugEntfallen {
            ZvfDokumentRef = dokumentRef,
            Zugnr          = (int)e.Zugnr,
            Zugbez         = e.Zugbez,
            Verkehrstag    = e.Verkehrstag,
            Art            = e.RegelungsartAlt
        };

        _stats.EntfallenInserted++;
        db.ZvfDokumentZugEntfallen.Add(entity);
        return Task.CompletedTask;

        // ❗ KEIN SaveChanges
        // ❗ KEIN AnyAsync
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
        // -------------------------------------------------
        // Fast Lookup (read-only)
        // -------------------------------------------------
        var existing = await db.ZvfDokument
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

        var regionRef = await resolver.ResolveRegionAsync(
            db,
            dto.Document.MasterRegion,
            token);

        if (regionRef <= 0)
            throw new InvalidOperationException(
                $"Region '{dto.Document.MasterRegion}' konnte nicht aufgelöst werden");

        // -------------------------------------------------
        // Create
        // -------------------------------------------------
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
        // -------------------------------------------------
        // 1) Exists?
        // -------------------------------------------------
        var existing = await db.ZvfDokumentZug
            .AsTracking()
            .FirstOrDefaultAsync(z =>
                    z.ZvfDokumentRef == dokumentRef   &&
                    z.Zugnr          == zug.Zugnummer &&
                    z.Verkehrstag    == zug.Verkehrstag,
                token);

        // -------------------------------------------------
        // 2) Referenzen auflösen (FKs sind NOT NULL)
        // -------------------------------------------------
        var kundeRef = await resolver.ResolveOrCreateKundeAsync(
            db,
            zug.Betreiber,
            token);

        var abgangBstRef = await resolver.ResolveOrCreateBetriebsstelleAsync(
            db,
            zug.Regelweg?.Abgangsbahnhof?.Ds100 ?? string.Empty,
            token);

        var zielBstRef = await resolver.ResolveOrCreateBetriebsstelleAsync(
            db,
            zug.Regelweg?.Zielbahnhof?.Ds100 ?? string.Empty,
            token);

        // -------------------------------------------------
        // 3) INSERT
        // -------------------------------------------------
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
            await db.SaveChangesAsync(token);

            return entity.Id;
        }

        // -------------------------------------------------
        // 4) UPDATE (nur bei echten Änderungen)
        // -------------------------------------------------
        SetIfDifferent(existing.Zugbez,    zug.Zugbez,    v => existing.Zugbez    = v);
        SetIfDifferent(existing.Aenderung, zug.Aenderung, v => existing.Aenderung = v);
        SetIfDifferent(existing.RegelwegLinie, zug.Regelweg?.LinienNr ?? string.Empty,
            v => existing.RegelwegLinie = v);
        SetIfDifferent(existing.Klv,       zug.Klv,         v => existing.Klv       = v);
        SetIfDifferent(existing.Skl,       zug.Skl,         v => existing.Skl       = v);
        SetIfDifferent(existing.Bza,       zug.Bza,         v => existing.Bza       = v);
        SetIfDifferent(existing.Bemerkung, zug.Bemerkungen, v => existing.Bemerkung = v);
        SetIfDifferent(existing.Bedarf,    zug.Bedarf,      v => existing.Bedarf    = v);
        SetIfDifferent(existing.Sonderzug, zug.Sonder,      v => existing.Sonderzug = v);

        // FK-Refs
        SetIfDifferent(existing.KundeRef,             kundeRef,     v => existing.KundeRef             = v);
        SetIfDifferent(existing.RegelwegAbgangBstRef, abgangBstRef, v => existing.RegelwegAbgangBstRef = v);
        SetIfDifferent(existing.RegelwegZielBstRef,   zielBstRef,   v => existing.RegelwegZielBstRef   = v);

        _stats.ZuegeUpdated++;
        return existing.Id;
    }


    // =====================================================================
    // Abweichung
    // =====================================================================
    private async Task UpsertAbweichungAsync(
        UjBauDbContext    db,
        ZvFZugAbweichung  abw,
        long              zugRef,
        CancellationToken token) {
        long? ankerBstRef = null;

        if (!string.IsNullOrWhiteSpace(abw.AnchorRl100)) {
            ankerBstRef = await resolver.ResolveOrCreateBetriebsstelleAsync(
                db,
                abw.AnchorRl100,
                token);
        }

        if (ankerBstRef is null or <= 0) {
            Logger.Warn(
                "Regelung ohne RL100-Anker erkannt | Art={Art}, Zug={Zug}, Tag={Tag}, AnchorRl100='{Ab}', Json={Json}",
                abw.Regelungsart,
                abw.Zugnummer,
                abw.Verkehrstag.ToString("yyyy-MM-dd"),
                abw.AnchorRl100,
                abw.JsonRaw
            );

        }

        var entity = new ZvfDokumentZugAbweichung {
            ZvfDokumentZugRef = zugRef,
            Art               = abw.Regelungsart,
            Abweichung        = abw.JsonRaw,
            AbBstRef          = ankerBstRef
        };

        _stats.AbweichungenInserted++;
        db.ZvfDokumentZugAbweichung.Add(entity);
        // ❗ KEIN SaveChanges hier
        // ❗ UniqueConstraint + zentraler SaveChanges reichen aus
    }

    // =====================================================================
    // Helper
    // =====================================================================
    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

    private static void SetIfDifferent<T>(T current,
        T                                   next,
        Action<T>                           setter) {
        if (EqualityComparer<T>.Default.Equals(current, next)) return;

        setter(next);
    }
}