using BauFahrplanMonitor.Core.Importer.Dto.Nfpl;
using BauFahrplanMonitor.Core.Importer.Helper;
using BauFahrplanMonitor.Core.Interfaces;
using BauFahrplanMonitor.Core.Resolver;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Helpers;
using BauFahrplanMonitor.Models;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace BauFahrplanMonitor.Core.Importer.Upsert;

public sealed class NetzfahrplanUpserter(SharedReferenceResolver resolver) : INetzfahrplanUpserter {
    private readonly        SharedReferenceResolver _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    private static readonly Logger                  Logger    = LogManager.GetCurrentClassLogger();

    public async Task UpsertAsync(
        UjBauDbContext                 db,
        NetzfahrplanDto                dto,
        INfplZugCache                  zugCache,
        IProgress<UpsertProgressInfo>? progress,
        CancellationToken              token) {

        var totalZuege = dto.Zuege.Count;
        var index      = 0;

        db.ChangeTracker.AutoDetectChangesEnabled = false;

        foreach (var zug in dto.Zuege) {

            token.ThrowIfCancellationRequested();
            index++;

            // 🔑 Progress analog ZvF
            if (index % 5 == 0 || index == totalZuege) {
                progress?.Report(new UpsertProgressInfo {
                    Phase   = UpsertPhase.Zuege,
                    Current = index,
                    Total   = totalZuege
                });
            }

            var zugEntry = await zugCache.GetOrCreateAsync(
                zug.ZugNr,
                dto.FahrplanJahr);

            await zugEntry.Lock.WaitAsync(token);
            try {
                if (zugEntry.ZugId <= 0) {
                    var zugEntity = await UpsertZugAsync(db, zug.ZugNr, dto.FahrplanJahr, token);
                    await db.SaveChangesAsync(token);       // 🔑 jetzt ist die ID sicher
                    zugEntry.ZugId = zugEntity.Id;
                    db.ChangeTracker.Clear();
                }

                foreach (var variante in zug.Varianten) {
                    await UpsertVarianteAsync(db, zugEntry, zug.ZugNr, variante, token);
                }
            }
            finally {
                zugEntry.Lock.Release();
            }

        }

        try {
            await db.SaveChangesAsync(token);
            db.ChangeTracker.Clear();
        }
        catch (Exception ex) {
            Logger.Error($"Fehler beim Commit in DB: {ex.Message}\n{ex.InnerException}\n{ex.StackTrace}");
        }
    }

    // =====================================================================
    // nfpl_zug
    // =====================================================================
    private static async Task<NfplZug> UpsertZugAsync(
        UjBauDbContext    db,
        long              zugNr,
        int               fahrplanJahr,
        CancellationToken token) {

        var zug = await db.NfplZug
                      .SingleOrDefaultAsync(z => z.ZugNr == zugNr && z.FahrplanJahr == fahrplanJahr, token)
                  ?? new NfplZug();

        zug.ZugNr        = zugNr;
        zug.FahrplanJahr = fahrplanJahr;

        db.NfplZug.Update(zug);
        return zug;
    }

    // =====================================================================
    // nfpl_zug_variante + verlauf
    // =====================================================================

    private async Task UpsertVarianteAsync(
        UjBauDbContext             db,
        ZugCacheEntry              zugEntry,
        long                       zugNr,
        NetzfahrplanZugVarianteDto variante,
        CancellationToken          token) {

        if (zugEntry.ZugId <= 0) {
            throw new InvalidOperationException($"Zug nicht persistent: ZugNr={zugNr}");
        }

        // -------------------------------------------------
        // Variante-ID aus Cache oder DB
        // -------------------------------------------------
        var key = (variante.TrainId, variante.TrainNumber);

        if (!zugEntry.Varianten.TryGetValue(key, out var varianteId)) {

            var entity = await db.NfplZugVariante
                .Where(v =>
                    v.NfplZugRef  == zugEntry.ZugId   &&
                    v.TrainId     == variante.TrainId &&
                    v.TrainNumber == variante.TrainNumber)
                .SingleOrDefaultAsync(token);

            var isNew = entity == null;
            entity ??= new NfplZugVariante();

            entity.NfplZugRef  = zugEntry.ZugId;
            entity.TrainId     = variante.TrainId;
            entity.TrainNumber = variante.TrainNumber;
            entity.Kind        = variante.Kind;
            entity.Remarks     = variante.Remarks;
            entity.TrainStatus = variante.TrainStatus;

            if (!string.IsNullOrWhiteSpace(variante.Region)) {
                var regionRef = await _resolver.ResolveRegionAsync(db, variante.Region, token);
                entity.RegionRef = regionRef;
            }

            db.NfplZugVariante.Update(entity);

            if (isNew) {
                await db.SaveChangesAsync(token);
                db.ChangeTracker.Clear();
            }

            varianteId = entity.Id;
            zugEntry.Varianten.TryAdd(key, varianteId);
        }

        // -------------------------------------------------
        // Verlauf: DELETE + INSERT (bewusst!
        // -------------------------------------------------
        if (varianteId <= 0) {
            throw new InvalidOperationException($"Variante ohne ID: ZugNr={zugNr}, TrainId={variante.TrainId}, TrainNumber={variante.TrainNumber}");
        }

        // 🔥 1) Altbestand löschen
        await db.NfplZugVarianteVerlauf
            .Where(v => v.NfplZugVarRef == varianteId)
            .ExecuteDeleteAsync(token);

        // 🔁 2) Neu aufbauen
        long seq = 1;

        string?   lastBitmask            = null;
        DateOnly? lastStartDate          = null;
        DateOnly? lastEndDate            = null;
        string?   lastServiceDescription = null;

        //Logger.Info($"Dump: \n{variante.Dump()}");
        if (variante.Verlauf.Count > 1) {
            var inserts = new List<NfplZugVarianteVerlauf>(variante.Verlauf.Count);

            foreach (var v in variante.Verlauf) {

                token.ThrowIfCancellationRequested();

                // -------------------------------------------------
                // Service-Zustand fortschreiben
                // -------------------------------------------------
                if (!string.IsNullOrWhiteSpace(v.ServiceBitmask))
                    lastBitmask = v.ServiceBitmask;

                if (v.ServiceStartdate != null)
                    lastStartDate = v.ServiceStartdate;

                if (v.ServiceEnddate != null)
                    lastEndDate = v.ServiceEnddate;

                if (!string.IsNullOrWhiteSpace(v.ServiceDescription))
                    lastServiceDescription = v.ServiceDescription;

                // -------------------------------------------------
                // Betriebsstelle
                // -------------------------------------------------
                if (string.IsNullOrWhiteSpace(v.BstRl100)) {
                    Logger.Warn(
                        "Verlauf ohne RL100 übersprungen: ZugNr={ZugNr}, TrainNumber={TrainNumber}, TrainId={TrainId}, Seq={Seq}",
                        zugNr,
                        variante.TrainNumber,
                        variante.TrainId,
                        seq);
                    continue;
                }

                var bstRef =
                    await _resolver.ResolveOrCreateBetriebsstelleCachedAsync(
                        db,
                        v.BstRl100,
                        token);

                if (bstRef == -1) {
                    Logger.Warn(
                        "Verlauf ohne RL100-Referenz übersprungen: ZugNr={ZugNr}, TrainNumber={TrainNumber}, TrainId={TrainId}, Seq={Seq}",
                        zugNr,
                        variante.TrainNumber,
                        variante.TrainId,
                        seq);
                    continue;
                }

                inserts.Add(
                    new NfplZugVarianteVerlauf {
                        NfplZugVarRef      = varianteId,
                        Seq                = seq++,
                        BstRef             = bstRef,
                        Type               = v.Type,
                        PublishedArrival   = v.PublishedArrival,
                        PublishedDeparture = v.PublishedDeparture,
                        Remarks            = v.Remarks,
                        ServiceBitmask     = lastBitmask,
                        ServiceStartdate   = lastStartDate,
                        ServiceEnddate     = lastEndDate,
                        ServiceDescription = lastServiceDescription
                    });
            }

            db.NfplZugVarianteVerlauf.AddRange(inserts);
        }

        // 🔑 3) EIN SaveChanges
        //await db.SaveChangesAsync(token);
    }
}