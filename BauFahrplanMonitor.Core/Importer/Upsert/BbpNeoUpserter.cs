using System.Text.Json;
using BauFahrplanMonitor.Core.Importer.Dto.BbpNeo;
using BauFahrplanMonitor.Core.Interfaces;
using BauFahrplanMonitor.Core.Resolver;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Models;
using NLog;

namespace BauFahrplanMonitor.Core.Importer.Upsert;

public sealed class BbpNeoUpserter(SharedReferenceResolver resolver) : IBbpNeoUpserter {
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    // =====================================================================
    // ENTRY
    // =====================================================================
    public async Task UpsertMassnahmeWithChildrenAsync(
        UjBauDbContext        db,
        BbpNeoMassnahme       dto,
        IReadOnlyList<string> warnings,
        Action                onRegelungUpserted,
        Action                onBveUpserted,
        Action                onApsUpserted,
        Action                onIavUpserted,
        CancellationToken     token) {
        token.ThrowIfCancellationRequested();

        db.ChangeTracker.AutoDetectChangesEnabled = false;

        Logger.Info(
            "[BBPNeo.Upsert] ▶ Maßnahme START: MasNr={MasNr}",
            dto.MasId);

        // -------------------------------------------------
        // MASSNAHME
        // -------------------------------------------------

        var masVonBstRef     = await resolver.ResolveOrCreateBetriebsstelleAsync(db, dto.MasVonBstDs100, token);
        var masVonStrRef     = await resolver.ResolveStreckeAsync(db, dto.MasVonVzG);
        var masVonBst2StrRef = await resolver.ResolveBst2StrAsync(db, masVonBstRef, masVonStrRef, null, token);

        var masBisBstRef     = await resolver.ResolveOrCreateBetriebsstelleAsync(db, dto.MasBisBstDs100, token);
        var masBisStrRef     = await resolver.ResolveStreckeAsync(db, dto.MasBisVzG);
        var masBisBst2StrRef = await resolver.ResolveBst2StrAsync(db, masBisBstRef, masBisStrRef, null, token);

        var mn = db.BbpneoMassnahme.SingleOrDefault(s => s.MasId == dto.MasId) ?? new BbpneoMassnahme();
        mn.MasId            = dto.MasId;
        mn.Aktiv            = dto.Aktiv;
        mn.Arbeiten         = dto.Arbeiten     ?? string.Empty;
        mn.ArtDerArbeit     = dto.ArtDerArbeit ?? string.Empty;
        mn.RegionRef        = dto.RegionId     ?? throw new Exception("Konnte Region Referenz nicht auflösen");
        mn.MasVonBst2strRef = masVonBst2StrRef;
        mn.MasBisBst2strRef = masBisBst2StrRef;
        mn.MasVonKmL        = dto.MasVonKmL;
        mn.MasBisKmL        = dto.MasBisKmL;
        mn.Genehmigung      = dto.Genehmigung;
        mn.AnforderungBbzr  = dto.AnforderungBbzr;
        if (dto.Beginn.HasValue) mn.MasBeginn = dto.Beginn.Value;
        if (dto.Ende.HasValue) mn.MasEnde     = dto.Ende.Value;

        try {
            db.BbpneoMassnahme.Update(mn);
            await db.SaveChangesAsync(token);
        }
        catch (Exception ex) {
            Logger.Fatal($"Fehler beim Einfügen einer Maßnahme {dto.MasId} in die Datenbank: {ex.Message}\n{ex.InnerException}\n{ex.StackTrace}");
        }

        // Nun die Regelungen
        await UpsertRegelungenAsync(
            db,
            dto.Regelungen,
            mn.Id,
            onRegelungUpserted,
            onBveUpserted,
            onApsUpserted,
            onIavUpserted,
            token);

        Logger.Info(
            "[BBPNeo.Upsert] ✔ Maßnahme READY (SaveChanges ausstehend): MasNr={MasNr}",
            dto.MasId);
        db.ChangeTracker.AutoDetectChangesEnabled = true;
    }

    // ---------------------------
    // Regelungen
    // ---------------------------
    private async Task UpsertRegelungenAsync(
        UjBauDbContext                db,
        IReadOnlyList<BbpNeoRegelung> dtoRegelungen,
        long                          masRef,
        Action                        onRegelungUpserted,
        Action                        onBveUpserted,
        Action                        onApsUpserted,
        Action                        onIavUpserted,
        CancellationToken             token) {

        foreach (var regelung in dtoRegelungen) {
            var reg =
                db.BbpneoMassnahmeRegelung.SingleOrDefault(s =>
                    s.BbpneoMasRef == masRef &&
                    s.RegId        == regelung.RegId)
                ?? new BbpneoMassnahmeRegelung();

            var regVonBstRef     = await resolver.ResolveOrCreateBetriebsstelleAsync(db, regelung.VonBstDs100, token);
            var regVonStrRef     = await resolver.ResolveStreckeAsync(db, regelung.VonVzG);
            var regVonBst2StrRef = await resolver.ResolveBst2StrAsync(db, regVonBstRef, regVonStrRef, null, token);

            var regBisBstRef     = await resolver.ResolveOrCreateBetriebsstelleAsync(db, regelung.BisBstDs100, token);
            var regBisStrRef     = await resolver.ResolveStreckeAsync(db, regelung.BisVzG);
            var regBisBst2StrRef = await resolver.ResolveBst2StrAsync(db, regBisBstRef, regBisStrRef, null, token);

            reg.BbpneoMasRef  = masRef;
            reg.RegId         = regelung.RegId;
            reg.Aktiv         = regelung.Aktiv;
            reg.Bplart        = regelung.BplArt   ?? string.Empty;
            reg.Beginn        = regelung.Beginn   ?? throw new Exception($"Kein Beginndatum für RegID {regelung.RegId}");
            reg.Ende          = regelung.Ende     ?? throw new Exception($"Kein Enddatum für RegId {regelung.RegId}");
            reg.Zeitraum      = regelung.Zeitraum ?? string.Empty;
            reg.Richtung      = regelung.Richtung;
            reg.RegelungKurz  = regelung.RegelungKurz;
            reg.RegelungLang  = regelung.RegelungLang;
            reg.Durchgehend   = regelung.Durchgehend;
            reg.Schichtweise  = regelung.Schichtweise;
            reg.Bst2strVonRef = regVonBst2StrRef;
            reg.Bst2strBisRef = regBisBst2StrRef;

            try {
                db.BbpneoMassnahmeRegelung.Update(reg);
                await db.SaveChangesAsync(token);
                onRegelungUpserted();
            }
            catch (Exception ex) {
                Logger.Fatal($"Fehler beim Einfügen einer Regelung {regelung.RegId} in die Datenbank: {ex.Message}\n{ex.InnerException}\n{ex.StackTrace}");
            }

            // Nun die BvE(n)
            await UpsertBvenAsync(db, regelung.Bven, reg.Id, onBveUpserted, onApsUpserted, onIavUpserted, token);
        }
    }

    // ---------------------------
    // BvE(n)
    // ---------------------------
    private async Task UpsertBvenAsync(
        UjBauDbContext           db,
        IReadOnlyList<BbpNeoBve> regelungBven,
        long                     regId,
        Action                   onBveUpserted,
        Action                   onApsUpserted,
        Action                   onIavUpserted,
        CancellationToken        token) {

        foreach (var bve in regelungBven) {
            var b = db.BbpneoMassnahmeRegelungBve.SingleOrDefault(s =>
                        s.BbpneoMasRegRef == regId &&
                        s.BveId           == bve.BveId)
                    ?? new BbpneoMassnahmeRegelungBve();

            var bveVonBstRef     = await resolver.ResolveOrCreateBetriebsstelleAsync(db, bve.VonBstDs100, token);
            var bveVonStrRef     = await resolver.ResolveStreckeAsync(db, bve.VonVzG, token);
            var bveVonBst2StrRef = await resolver.ResolveBst2StrAsync(db, bveVonBstRef, bveVonStrRef, null, token);

            var bveBisBstRef     = await resolver.ResolveOrCreateBetriebsstelleAsync(db, bve.BisBstDs100, token);
            var bveBisStrRef     = await resolver.ResolveStreckeAsync(db, bve.BisVzG, token);
            var bveBisBst2StrRef = await resolver.ResolveBst2StrAsync(db, bveBisBstRef, bveBisStrRef, null, token);

            b.BveId                            = bve.BveId;
            b.BbpneoMasRegRef                  = regId;
            b.Aktiv                            = bve.Aktiv;
            b.Art                              = bve.Art;
            b.OrtMikroskopisch                 = bve.OrtMikroskopisch;
            b.Bemerkung                        = bve.Bemerkung;
            b.IavBetroffenheit                 = bve.Iav?.IstBetroffen      ?? false;
            b.ApsBetroffenheit                 = bve.Aps?.IstBetroffen      ?? false;
            b.IavBeschreibung                  = bve.Iav?.Beschreibung      ?? string.Empty;
            b.ApsBeschreibung                  = bve.Aps?.Beschreibung      ?? string.Empty;
            b.ApsFreiVonFahrzeugen             = bve.Aps?.FreiVonFahrzeugen ?? false;
            b.Gueltigkeit                      = bve.Gueltigkeit;
            b.GueltigkeitVon                   = bve.GueltigkeitVon;
            b.GueltigkeitBis                   = bve.GueltigkeitBis;
            b.GueltigkeitEffektiveVerkehrstage = bve.GueltigkeitEffektiveVerkehrstage;
            b.Bst2strVonRef                    = bveVonBst2StrRef;
            b.Bst2strBisRef                    = bveBisBst2StrRef;

            try {
                db.BbpneoMassnahmeRegelungBve.Update(b);
                await db.SaveChangesAsync(token);
                onBveUpserted();
            }
            catch (Exception ex) {
                Logger.Fatal($"Fehler beim Einfügen einer BvE {bve.BveId} in die Datenbank: {ex.Message}\n{ex.InnerException}\n{ex.StackTrace}");
            }

            if (bve.Aps?.IstBetroffen ?? false) {
                await UpdateAps(db, b.Id, bve.Aps, token);
                onApsUpserted();
            }

            if (!(bve.Iav?.IstBetroffen ?? false))
                continue;
            await UpdateIaV(db, b.Id, bve.Iav, token);
            onIavUpserted();
        }
    }
    
    private async Task UpdateAps(UjBauDbContext db, long bId, BbpNeoAps bbpBveAps, CancellationToken token) {

        foreach (var bveAps in bbpBveAps.Betroffenheiten) {
            var aps = db.BbpneoMassnahmeRegelungBveAps.SingleOrDefault(s =>
                          s.BbpneoMassnahmeRegelungBveRef == bId &&
                          s.Uuid                          == bveAps.Uuid)
                      ?? new BbpneoMassnahmeRegelungBveAps();

            var bstRef = await resolver.ResolveOrCreateBetriebsstelleAsync(db, bveAps.BstDs100, token);

            aps.BbpneoMassnahmeRegelungBveRef = bId;
            aps.AbFahrplanjahr                = bveAps.AbFahrplanjahr;
            aps.Uuid                          = bveAps.Uuid;
            aps.Gleis                         = bveAps.Gleis;
            aps.PrimaereKategorie             = bveAps.PrimaereKat;
            aps.SekundaereKategorie           = bveAps.SekundaerKat;
            aps.Oberleitung                   = bveAps.Oberleitung;
            aps.OberleitungAus                = bveAps.OberleitungAus;
            aps.TechnischerPlatz              = bveAps.TechnischerPlatz;
            aps.ArtDerAnbindung               = bveAps.ArtDerAnbindung;
            aps.EinschraenkungBefahrbarkeitSe = bveAps.EinschraenkungBefahrbarkeitSe;
            aps.Kommentar                     = bveAps.Kommentar;
            aps.BstRef                        = bstRef;
            
            aps.MoeglicheZa =
                bveAps.MoeglicheZas is { Count: > 0 }
                    ? JsonSerializer.Serialize(
                        bveAps.MoeglicheZas,
#pragma warning disable CA1869
                        new JsonSerializerOptions {
#pragma warning restore CA1869
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        })
                    : "[]";   // 🔑 NICHT string.Empty

            
            try {
                db.BbpneoMassnahmeRegelungBveAps.Update(aps);
                await db.SaveChangesAsync(token);
            }
            catch (Exception ex) {
                Logger.Fatal($"Fehler beim Einfügen einer APS {bveAps.Uuid} in die Datenbank: {ex.Message}\n{ex.InnerException}\n{ex.StackTrace}");
            }
        }
    }
    
    private async Task UpdateIaV(UjBauDbContext db, long bId, BbpNeoIav? bbpBveIaV, CancellationToken token) {

        foreach (var bveIav in bbpBveIaV!.Betroffenheiten) {
            var iav = db.BbpneoMassnahmeRegelungBveIav.SingleOrDefault(s =>
                          s.BbpneoMassnahmeRegelungBveRef == bId &&
                          s.VertragNr                     == bveIav.VertragNr)
                      ?? new BbpneoMassnahmeRegelungBveIav();

            var bstRef     = await resolver.ResolveOrCreateBetriebsstelleAsync(db, bveIav.BstDs100, token);
            var strRef     = await resolver.ResolveStreckeAsync(db, bveIav.VzgStrecke, token);
            var bst2StrRef = await resolver.ResolveBst2StrAsync(db, bstRef, strRef, null, token);

            iav.BbpneoMassnahmeRegelungBveRef = bId;
            iav.Anschlussgrenze               = bveIav.Anschlussgrenze;
            iav.VertragNr                     = bveIav.VertragNr;
            iav.VertragArt                    = bveIav.VertragArt;
            iav.VertragStatus                 = bveIav.VertragStatus;
            iav.Kunde                         = bveIav.Kunde;
            iav.Oberleitung                   = bveIav.Oberleitung;
            iav.OberleitungAus                = bveIav.OberleitungAus;
            iav.EinschraenkungBedienbarkeitIa = bveIav.EinschraenkungBedienbarkeitIA;
            iav.Kommentar                     = bveIav.Kommentar;
            iav.Bst2strRef                    = bst2StrRef;

            try {
                db.BbpneoMassnahmeRegelungBveIav.Update(iav);
                await db.SaveChangesAsync(token);
            }
            catch (Exception ex) {
                Logger.Fatal($"Fehler beim Einfügen einer IAV {bveIav.VertragNr} in die Datenbank: {ex.Message}\n{ex.InnerException}\n{ex.StackTrace}");
            }
        }
    }
}

