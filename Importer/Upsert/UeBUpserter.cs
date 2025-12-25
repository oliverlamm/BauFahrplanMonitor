using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Helpers;
using BauFahrplanMonitor.Importer.Dto.Ueb;
using BauFahrplanMonitor.Importer.Helper;
using BauFahrplanMonitor.Importer.Interface;
using BauFahrplanMonitor.Importer.Mapper;
using BauFahrplanMonitor.Models;
using BauFahrplanMonitor.Resolver;
using BauFahrplanMonitor.Services;
using Microsoft.EntityFrameworkCore;
using NLog;
using Npgsql;

namespace BauFahrplanMonitor.Importer.Upsert;

/// <summary>
/// Upserter für ÜB-Dokumente
/// Vollständig, eigenständig, FK-sicher
/// </summary>
public sealed class UeBUpserter(
    IDbContextFactory<UjBauDbContext> dbFactory,
    SharedReferenceResolver           resolver,
    ConfigService                     config)
    : ImportUpserterBase(config, LogManager.GetCurrentClassLogger()), IUeBUpserter {
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private UeBImportStats Stats { get; set; } = new();

    // =====================================================================
    // ENTRYPOINT
    // =====================================================================
    public async Task<UpsertResult> UpsertAsync(
        UjBauDbContext                db,
        UebXmlDocumentDto             dto,
        IProgress<UpsertProgressInfo> progress,
        CancellationToken             token) {
        token.ThrowIfCancellationRequested();

        Stats = new UeBImportStats { Dokumente = 1 };
        var sw = Stopwatch.StartNew();

        // -------------------------------------------------
        // PHASE A: Referenzen
        // -------------------------------------------------
        var vorgangRef = await resolver.ResolveOrCreateVorgangAsync(db, dto.Vorgang, ImportMode.UeB, token);
        var senderRef  = await resolver.ResolveOrCreateSenderAsync(db, dto.Header, token);

        await db.SaveChangesAsync(token);

        // -------------------------------------------------
        // PHASE B: Transaktion
        // -------------------------------------------------
        await using var tx = await db.Database.BeginTransactionAsync(token);

        try {
            var dokumentRef = await ResolveOrInsertDokumentAsync(db, dto, vorgangRef, senderRef, token);
            await UpsertStreckenabschnitteAsync(dto, db, dokumentRef, token);
            var factoryResult = UebZugFactory.Build(dto.Document);

            Stats.SevsGelesen              += factoryResult.SevsGelesen;
            Stats.SevsMitErsatzzug         += factoryResult.SevsMitErsatzzug;
            Stats.ZuegeAusSevErzeugt       += factoryResult.ZuegeAusSevErzeugt;
            Stats.ErsatzzuegeAusSevErzeugt += factoryResult.ErsatzzuegeAusSevErzeugt;

            var total = factoryResult.Zuege.Count;
            var index = 0;

            foreach (var zug in factoryResult.Zuege) {
                token.ThrowIfCancellationRequested();
                index++;

                progress?.Report(new UpsertProgressInfo {
                    Phase   = UpsertPhase.Zuege,
                    Current = index,
                    Total   = total
                });

                var zugRef = await ResolveOrInsertZugAsync(db, zug, dokumentRef, dto, token);

                await UpsertKnotenzeitenAsync(db, zug, zugRef, token);

                var key = new UebZugKey(zug.Zugnummer, zug.Verkehrstag);
                if (!factoryResult.Regelungen.TryGetValue(key, out var regs)) continue;
                foreach (var reg in regs) {
                    await UpsertRegelungAsync(db, reg, zugRef, token);
                }
            }

            await db.SaveChangesAsync(token);
            await tx.CommitAsync(token);

            sw.Stop();
            Logger.Info($"ÜB-Import abgeschlossen: Datei={dto.Document.Dateiname}, Dauer={sw.Elapsed}, Stats={Stats}");

            return new UpsertResult {
                DokumentRef = dokumentRef,
                UeBStats    = Stats
            };
        }
        catch (Exception ex) {
            await tx.RollbackAsync(CancellationToken.None);

            HandleException(ex, "UeBUpsert", new {
                dto.Document.Dateiname,
                dto.Vorgang.MasterFplo
            });

            throw;
        }
    }

    // =====================================================================
    // Exit
    // =====================================================================
    public async Task MarkImportCompletedAsync(long dokumentRef, CancellationToken token) {
        await using var db = await dbFactory.CreateDbContextAsync(token);

        var doc = await db.UebDokument.FindAsync([dokumentRef], token);
        if (doc == null)
            return;

        doc.ImportTimestamp = DateTime.Now;

        await db.SaveChangesAsync(token);
    }

    // =====================================================================
    // ZUG
    // =====================================================================
    private async Task<long> ResolveOrInsertZugAsync(
        UjBauDbContext    db,
        UebZugDto         zug,
        long              dokumentRef,
        UebXmlDocumentDto dto,
        CancellationToken token) {
        var existing =
            await db.UebDokumentZug.FirstOrDefaultAsync(
                z =>
                    z.UebDokumentRef == dokumentRef   &&
                    z.Zugnr          == zug.Zugnummer &&
                    z.Verkehrstag    == zug.Verkehrstag,
                token);

        var kundeRef = await ResolveKundeAsync(db, zug, dto, token);

        if (existing != null) {
            existing.KundeRef = kundeRef;
            Stats.ZuegeUpdated++;
            return existing.Id;
        }

        var entity = new UebDokumentZug {
            UebDokumentRef = dokumentRef,
            Zugnr          = zug.Zugnummer,
            Verkehrstag    = zug.Verkehrstag,
            Bedarf         = zug.Bedarf,
            KundeRef       = kundeRef
        };

        db.UebDokumentZug.Add(entity);
        await db.SaveChangesAsync(token);

        Stats.ZuegeInserted++;
        return entity.Id;
    }

    // =====================================================================
    // KUNDE
    // =====================================================================
    private async Task<long> ResolveKundeAsync(
        UjBauDbContext    db,
        UebZugDto         zug,
        UebXmlDocumentDto dto,
        CancellationToken token) {
        if (!string.IsNullOrWhiteSpace(zug.Betreiber))
            return await resolver.ResolveOrCreateKundeAsync(
                db, zug.Betreiber, token);

        var empfaenger = dto.Header?.Empfaenger;
        if (empfaenger is { Count: 1 }) {
            var kbez = empfaenger[0];
            var id = await db.BasisKunde
                .Where(k => k.Kbez == kbez)
                .Select(k => k.Id)
                .OrderBy(x => x)
                .FirstOrDefaultAsync(token);

            if (id > 0) {
                Stats.SevKundeRefFallbackHeader++;
                return id;
            }
        }

        Stats.SevKundeRefFallbackZero++;
        return 0;
    }

    // =====================================================================
    // Document
    // =====================================================================
    private async Task<long> ResolveOrInsertDokumentAsync(
        UjBauDbContext    db,
        UebXmlDocumentDto dto,
        long              vorgangRef,
        long?             senderRef,
        CancellationToken token) {
        // -------------------------------------------------
        // Fast Lookup (read-only)
        // -------------------------------------------------
        var existing = await db.UebDokument
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
        if (string.IsNullOrWhiteSpace(dto.Document.Masterniederlassung))
            throw new InvalidOperationException("Masterniederlassung fehlt");

        var regionRef = await resolver.ResolveRegionAsync(
            db,
            dto.Document.Masterniederlassung,
            token);

        if (regionRef <= 0)
            throw new InvalidOperationException(
                $"Region '{dto.Document.Masterniederlassung}' konnte nicht aufgelöst werden");

        // -------------------------------------------------
        // Create
        // -------------------------------------------------
        var doc = new UebDokument() {
            UjbauVorgangRef = vorgangRef,
            SenderRef       = senderRef ?? throw new InvalidOperationException("SenderRef fehlt"),
            ExportTimestamp = dto.Header.Timestamp,

            VersionMajor = dto.Document.Version.Major,
            VersionMinor = dto.Document.Version.Minor,
            VersionSub   = dto.Document.Version.Sub,
            Version      = dto.Document.Version.VersionNumeric ?? 0,

            RegionRef      = regionRef,
            GueltigkeitVon = dto.Document.GueltigAb,
            GueltigkeitBis = dto.Document.GueltigBis,
            Allgemein      = dto.Document.AllgemeinText,
            Dateiname      = dto.Document.Dateiname,
        };

        db.UebDokument.Add(doc);

        try {
            await db.SaveChangesAsync(token);
            return doc.Id;
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex)) {
            var winnerId = await db.UebDokument
                .Where(d =>
                    d.UjbauVorgangRef == vorgangRef &&
                    d.Dateiname       == dto.Header.FileName)
                .Select(d => d.Id)
                .SingleOrDefaultAsync(token);

            if (winnerId > 0)
                return winnerId;

            throw new InvalidOperationException(
                "UebDokument konnte nach UniqueViolation nicht erneut gelesen werden", ex);
        }
    }

    // =====================================================================
    // STRECKEN
    // =====================================================================
    private async Task UpsertStreckenabschnitteAsync(
        UebXmlDocumentDto dto,
        UjBauDbContext    db,
        long              dokumentRef,
        CancellationToken token) {
        // bewusst leer – ÜB nutzt aktuell keine Streckenabschnitte
        await Task.CompletedTask;
    }

    // =====================================================================
    // KNOTENZEITEN
    // =====================================================================
    private async Task UpsertKnotenzeitenAsync(
        UjBauDbContext    db,
        UebZugDto         zug,
        long              zugRef,
        CancellationToken token) {
        if (zug.Knotenzeiten is not { Count: > 0 })
            return;

        await db.UebDokumentZugKnotenzeiten
            .Where(x => x.UebDokumentZugRef == zugRef)
            .ExecuteDeleteAsync(token);

        long lfd = 0;

        foreach (var k in zug.Knotenzeiten.OrderBy(x => x.RelativLage)) {
            lfd++;

            long? bstRef = null;
            if (!string.IsNullOrWhiteSpace(k.BahnhofDs100))
                bstRef = await resolver.ResolveOrCreateBetriebsstelleAsync(
                    db, k.BahnhofDs100, token);

            db.UebDokumentZugKnotenzeiten.Add(
                new UebDokumentZugKnotenzeiten {
                    UebDokumentZugRef = zugRef,
                    Lfdnr             = lfd,
                    Relativlage       = k.RelativLage,
                    BstRef            = bstRef
                });
        }
    }

    // =====================================================================
    // REGELUNG (DETAIL)
    // =====================================================================
    private async Task UpsertRegelungAsync(
        UjBauDbContext    db,
        UebRegelungDto    regelung,
        long              zugRef,
        CancellationToken token) {
        
        var ankerBstRef =
            await resolver.ResolveOrCreateBetriebsstelleAsync(
                db, regelung.AnchorRl100!, token);

        var entity = await db.UebDokumentZugRegelung
            .SingleOrDefaultAsync(
                r =>
                    r.UebDokumentZugRef == zugRef       &&
                    r.Art               == regelung.Art &&
                    r.AnkerBstRef       == ankerBstRef,
                token);

        if (entity != null) {
            // 🔁 MERGE
            entity.Regelung  ??= regelung.JsonRaw;
            
            return;
        }

        db.UebDokumentZugRegelung.Add(
            new UebDokumentZugRegelung {
                UebDokumentZugRef = zugRef,
                Art               = regelung.Art,
                AnkerBstRef       = ankerBstRef,
                Regelung          = regelung.JsonRaw
            });

        Stats.RegelungenInserted++;
    }

    // =====================================================================
    // Helper
    // =====================================================================
    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}