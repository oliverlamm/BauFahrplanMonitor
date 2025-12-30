using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Importer.Dto.BbpNeo;
using BauFahrplanMonitor.Interfaces;
using BauFahrplanMonitor.Models;
using BauFahrplanMonitor.Resolver;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace BauFahrplanMonitor.Importer.Upsert;

public sealed class BbpNeoUpserter(SharedReferenceResolver resolver) : IBbpNeoUpserter {
    private static readonly Logger                  Logger    = LogManager.GetCurrentClassLogger();

    // =====================================================================
    // ENTRY
    // =====================================================================
    public async Task UpsertMassnahmeWithChildrenAsync(
        UjBauDbContext        db,
        BbpNeoMassnahme       domain,
        IReadOnlyList<string> warnings,
        Action                onRegelungUpserted,
        Action                onBveUpserted,
        Action                onApsUpserted,
        Action                onIavUpserted,
        CancellationToken     token) {
        token.ThrowIfCancellationRequested();

        // 🔑 Import-Optimierung (einmalig, hier ok)
        db.ChangeTracker.AutoDetectChangesEnabled = false;

        Logger.Info(
            "[BBPNeo.Upsert] ▶ Maßnahme START: MasNr={MasNr}",
            domain.MasId);

        // -------------------------------------------------
        // MASSNAHME
        // -------------------------------------------------
        Logger.Info("ENTER UpsertMassnahmeAsync");
        var masEntity = await UpsertMassnahmeAsync(db, domain, token);
        await db.SaveChangesAsync(token);
        db.ChangeTracker.Clear();
        Logger.Info("EXIT  UpsertMassnahmeAsync");

        // -------------------------------------------------
        // REGELUNGEN + CHILDREN
        // -------------------------------------------------
        foreach (var regelung in domain.Regelungen) {
            token.ThrowIfCancellationRequested();

            Logger.Info("ENTER UpsertRegelungAsync");
            var regEntityId = await UpsertRegelungAsync(db, masEntity.Id, regelung, token);
            await db.SaveChangesAsync(token);
            db.ChangeTracker.Clear();
            Logger.Info("EXIT  UpsertRegelungAsync");
            
            onRegelungUpserted();

            // -------------------------
            // BVE / APS / IAV
            // -------------------------
            foreach (var bve in regelung.Bven) {
                token.ThrowIfCancellationRequested();

                var bveId = await UpsertBveAsync(db, regEntityId, bve, token);
                await db.SaveChangesAsync(token);
                db.ChangeTracker.Clear();
                onBveUpserted();

                if (bve.Aps?.Betroffenheit == true) {
                    await UpsertApsAsync(db, bveId, bve.Aps, token);
                    await db.SaveChangesAsync(token);
                    db.ChangeTracker.Clear();
                    onApsUpserted();
                }

                if (bve.Iav?.Betroffenheit == true) {
                    await UpsertIavAsync(db, bveId, bve.Iav, token);
                    await db.SaveChangesAsync(token);
                    db.ChangeTracker.Clear();
                    onIavUpserted();
                }
            }
        }

        Logger.Info(
            "[BBPNeo.Upsert] ✔ Maßnahme READY (SaveChanges ausstehend): MasNr={MasNr}",
            domain.MasId);
    }
    
    // =====================================================================
    // MASSNAHME
    // =====================================================================
    private async Task<BbpneoMassnahme> UpsertMassnahmeAsync(
        UjBauDbContext    db,
        BbpNeoMassnahme   mas,
        CancellationToken token) {
        var entity = await db.BbpneoMassnahme
            .FirstOrDefaultAsync(x => x.MasId == mas.MasId, token);

        if (entity == null) {
            entity = new BbpneoMassnahme {
                MasId = mas.MasId
            };
            db.Add(entity);
        }

        var bstVon = await resolver.ResolveOrCreateBetriebsstelleAsync(db, mas.MasVonBstDs100, token);
        var bstBis = await resolver.ResolveOrCreateBetriebsstelleAsync(db, mas.MasBisBstDs100, token);
        var strVon = await resolver.ResolveStreckeAsync(db, mas.MasVonVzG);
        var strBis = await resolver.ResolveStreckeAsync(db, mas.MasBisVzG);

        entity.Aktiv            = mas.Aktiv;
        entity.MasBeginn        = mas.Beginn    ?? DateTime.MinValue;
        entity.MasEnde          = mas.Ende      ?? DateTime.MinValue;
        entity.RegionRef        = mas.RegionRef ?? 0;
        entity.Arbeiten         = mas.Arbeiten  ?? string.Empty;
        entity.ArtDerArbeit     = mas.ArtDerArbeit;
        entity.Genehmigung      = mas.Genehmigung;
        entity.AnforderungBbzr  = mas.AnforderungBbzr;
        entity.MasVonBst2strRef = await resolver.ResolveBst2StrAsync(db, bstVon, strVon, token: token);
        entity.MasBisBst2strRef = await resolver.ResolveBst2StrAsync(db, bstBis, strBis, token: token);
        entity.MasVonKmL        = mas.MasVonKmL;
        entity.MasBisKmL        = mas.MasBisKmL;

        return entity;
    }

    // =====================================================================
    // REGELUNG
    // =====================================================================
    private async Task<long> UpsertRegelungAsync(
        UjBauDbContext    db,
        long              masRef,
        BbpNeoRegelung    reg,
        CancellationToken token) {
        var entity = await db.BbpneoMassnahmeRegelung
            .FirstOrDefaultAsync(x => x.RegId == reg.RegId, token);

        if (entity == null) {
            entity = new BbpneoMassnahmeRegelung {
                RegId        = reg.RegId,
                BbpneoMasRef = masRef
            };
            db.Add(entity);
        }

        var bstVon = await resolver.ResolveOrCreateBetriebsstelleAsync(db, reg.VonBstDs100, token);
        var bstBis = await resolver.ResolveOrCreateBetriebsstelleAsync(db, reg.BisBstDs100, token);
        var strVon = await resolver.ResolveStreckeAsync(db, reg.VonVzG);
        var strBis = await resolver.ResolveStreckeAsync(db, reg.BisVzG);

        entity.Aktiv         = reg.Aktiv;
        entity.Beginn        = reg.Beginn   ?? DateTime.MinValue;
        entity.Ende          = reg.Ende     ?? DateTime.MinValue;
        entity.Bplart        = reg.BplArt   ?? string.Empty;
        entity.Zeitraum      = reg.Zeitraum ?? string.Empty;
        entity.Richtung      = reg.Richtung ?? 0;
        entity.RegelungKurz  = reg.RegelungKurz;
        entity.RegelungLang  = reg.RegelungLang;
        entity.Durchgehend   = reg.Durchgehend  ?? false;
        entity.Schichtweise  = reg.Schichtweise ?? false;
        entity.Bst2strVonRef = await resolver.ResolveBst2StrAsync(db, bstVon, strVon, token: token);
        entity.Bst2strBisRef = await resolver.ResolveBst2StrAsync(db, bstBis, strBis, token: token);

        foreach (var bve in reg.Bven) {
            await UpsertBveAsync(db, entity.Id, bve, token);
        }

        return entity.Id;
    }

    // =====================================================================
    // BVE
    // =====================================================================
    private async Task<long> UpsertBveAsync(
        UjBauDbContext    db,
        long              regRef,
        BbpNeoBve         dto,
        CancellationToken token) {
        var bve = await db.BbpneoMassnahmeRegelungBve
            .FirstOrDefaultAsync(
                x => x.BbpneoMasRegRef == regRef &&
                     x.BveId           == dto.BveId,
                token);

        if (bve == null) {
            bve = new BbpneoMassnahmeRegelungBve {
                BbpneoMasRegRef = regRef,
                BveId           = dto.BveId
            };
            db.Add(bve);
        }

        bve.Aktiv            = dto.Aktiv;
        bve.Art              = dto.Art;
        bve.OrtMikroskopisch = dto.OrtMikroskopisch;
        bve.Bemerkung        = dto.Bemerkung;
        bve.GueltigkeitVon   = dto.GueltigkeitVon;
        bve.GueltigkeitBis   = dto.GueltigkeitBis;
        bve.Gueltigkeit      = dto.Gueltigkeit;
        bve.GueltigkeitEffektiveVerkehrstage =
            dto.GueltigkeitEffektiveVerkehrstage;

        var bstVonRef = await resolver.ResolveOrCreateBetriebsstelleAsync(db, dto.VonBstDs100, token);
        var strVonRef = await resolver.ResolveStreckeAsync(db, dto.VonVzG);
        bve.Bst2strVonRef = await resolver.ResolveBst2StrAsync(db, bstVonRef, strVonRef, token: token);
        var bstBisRef = await resolver.ResolveOrCreateBetriebsstelleAsync(db, dto.BisBstDs100, token);
        var strBisRef = await resolver.ResolveStreckeAsync(db, dto.BisVzG);
        bve.Bst2strBisRef = await resolver.ResolveBst2StrAsync(db, bstBisRef, strBisRef, token: token);


        bve.ApsBetroffenheit     = dto.Aps != null;
        bve.ApsBeschreibung      = dto.Aps?.Beschreibung;
        bve.ApsFreiVonFahrzeugen = dto.Aps?.FreiVonFahrzeugen ?? false;
        bve.IavBetroffenheit     = dto.Iav != null;
        bve.IavBeschreibung      = dto.Iav?.Beschreibung;

        return bve.Id;
    }

    // =====================================================================
    // APS
    // =====================================================================
    private async Task UpsertApsAsync(
        UjBauDbContext    db,
        long              bveRef,
        BbpNeoAps         aps,
        CancellationToken token) {

        foreach (var dto in aps.Betroffenheiten) {

            // 🔑 FACHLICHER FILTER
            if (!dto.IstBetroffen)
                continue;

            if (string.IsNullOrWhiteSpace(dto.Uuid))
                continue;

            var entity = await db.BbpneoMassnahmeRegelungBveAps
                .FirstOrDefaultAsync(
                    x => x.BbpneoMassnahmeRegelungBveRef == bveRef &&
                         x.Uuid                          == dto.Uuid,
                    token);

            if (entity == null) {
                entity = new BbpneoMassnahmeRegelungBveAps {
                    BbpneoMassnahmeRegelungBveRef = bveRef,
                    Uuid                          = dto.Uuid
                };
                db.Add(entity);
            }

            entity.Gleis                         = dto.Gleis;
            entity.PrimaereKategorie             = dto.PrimaereKat;
            entity.SekundaereKategorie           = dto.SekundaerKat ?? string.Empty;
            entity.Oberleitung                   = dto.Oberleitung;
            entity.OberleitungAus                = dto.OberleitungAus;
            entity.TechnischerPlatz              = dto.TechnischerPlatz;
            entity.ArtDerAnbindung               = dto.ArtDerAnbindung;
            entity.EinschraenkungBefahrbarkeitSe = dto.EinschraenkungBefahrbarkeitSe;
            entity.Kommentar                     = dto.Kommentar;
            entity.AbFahrplanjahr                = dto.AbFahrplanjahr;

            entity.BstRef = await resolver.ResolveOrCreateBetriebsstelleAsync(
                db, dto.BstDs100, token);

            entity.MoeglicheZa =
                dto.MoeglicheZAs is { Count: > 0 }
                    ? JsonSerializer.Serialize(
                        dto.MoeglicheZAs,
                        new JsonSerializerOptions {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        })
                    : string.Empty;


        }
    }

    // =====================================================================
    // IAV
    // =====================================================================
    private async Task UpsertIavAsync(
        UjBauDbContext    db,
        long              bveRef,
        BbpNeoIav         iav,
        CancellationToken token) {

        foreach (var dto in iav.Betroffenheiten) {
            // 🔑 FACHLICHER FILTER
            if (!dto.IstBetroffen)
                return;

            if (string.IsNullOrWhiteSpace(dto.VertragNr))
                return;

            var entity = await db.BbpneoMassnahmeRegelungBveIav
                .FirstOrDefaultAsync(
                    x => x.BbpneoMassnahmeRegelungBveRef == bveRef &&
                         x.VertragNr                     == dto.VertragNr,
                    token);

            if (entity == null) {
                entity = new BbpneoMassnahmeRegelungBveIav {
                    BbpneoMassnahmeRegelungBveRef = bveRef,
                    VertragNr                     = dto.VertragNr
                };
                db.Add(entity);
            }

            var bstRef = await resolver.ResolveOrCreateBetriebsstelleAsync(
                db, dto.BstDs100, token);

            var strRef = await resolver.ResolveStreckeAsync(
                db, dto.VzgStrecke);

            var bst2StrRef = await resolver.ResolveBst2StrAsync(
                db, bstRef, strRef, token: token);

            entity.Kunde                         = dto.Kunde;
            entity.Anschlussgrenze               = dto.Anschlussgrenze;
            entity.Oberleitung                   = dto.Oberleitung    ?? false;
            entity.OberleitungAus                = dto.OberleitungAus ?? false;
            entity.EinschraenkungBedienbarkeitIa = dto.EinschraenkungBedienbarkeitIA;
            entity.Kommentar                     = dto.Kommentar;
            entity.Bst2strRef                    = bst2StrRef;
            entity.VertragArt                    = dto.VertragArt;
            entity.VertragStatus                 = dto.VertragStatus;

        }
    }
}