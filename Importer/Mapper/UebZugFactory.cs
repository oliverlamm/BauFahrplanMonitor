using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using BauFahrplanMonitor.Importer.Dto.Ueb;
using BauFahrplanMonitor.Importer.Helper;
using BauFahrplanMonitor.Importer.Xml;
using NLog;

namespace BauFahrplanMonitor.Importer.Mapper;

public static class UebZugFactory {
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static UebZugFactoryResult Build(UebDocumentDto document) {
        var zugMap     = new Dictionary<UebZugKey, UebZugDto>();
        var regelungen = new Dictionary<UebZugKey, List<UebRegelungDto>>();

        // =================================================
        // PHASE 1 – ALLE <zug> EINLESEN (Primärquelle)
        // =================================================
        foreach (var zug in document.Zuege) {
            if (zug.Zugnummer <= 0 || zug.Verkehrstag == default) {
                Logger.Warn(
                    "ÜB-Factory: Ungültiger <zug> übersprungen: Zug={0}, VT={1}",
                    zug.Zugnummer,
                    zug.Verkehrstag);
                continue;
            }

            var key = new UebZugKey(zug.Zugnummer, zug.Verkehrstag);

            zugMap[key]     = zug;
            regelungen[key] = [];

            Logger.Debug(
                "ÜB-Factory: <zug> übernommen: Zug={0}/{1}, Abschnitt={2}",
                zug.Zugnummer,
                zug.Verkehrstag,
                zug.FploAbschnitt);

            // Komplettausfall-Regelung direkt aus <zug>
            if (!IsKomplettausfall(zug)) continue;
            var reg = CreateKomplettausfallRegelung(zug);
            AddOrMergeRegelung(regelungen[key], reg, key);
        }

        // =================================================
        // PHASE 2 – SEV AUSWERTEN (NUR Regelungen!)
        // =================================================
        foreach (var sev in document.Sev) {
            if (sev.ZugNr <= 0 || sev.Verkehrstag == null) {
                Logger.Warn(
                    "ÜB-Factory: Ungültiger <sev> übersprungen: Zug={0}, VT={1}",
                    sev.ZugNr,
                    sev.Verkehrstag);
                continue;
            }

            var key = new UebZugKey(sev.ZugNr, sev.Verkehrstag.Value);

            // Falls kein <zug> existiert → Minimalzug aus SEV
            if (!zugMap.TryGetValue(key, out var zug)) {
                Logger.Info(
                    "ÜB-Factory: Zug nur aus <sev> bekannt → Minimalzug: Zug={0}/{1}",
                    sev.ZugNr,
                    sev.Verkehrstag);

                zug             = CreateZugFromSev(sev);
                zugMap[key]     = zug;
                regelungen[key] = [];
            }

            // Komplettausfall schlägt SEV
            if (IsKomplettausfall(zug)) {
                Logger.Debug(
                    "ÜB-Factory: SEV ignoriert (Ausfall): Zug={0}/{1}",
                    zug.Zugnummer,
                    zug.Verkehrstag);
                continue;
            }

            // -------- Teilausfall-Regelung --------
            var teilAusfall = CreateTeilAusfallRegelung(sev);
            AddOrMergeRegelung(regelungen[key], teilAusfall, key);

            // -------- KEIN impliziter Ersatzzug! --------
            if (sev.Ersatzzug == null)
                continue;

            // Ersatzzug NUR wenn explizit vorhanden
            var ez = sev.Ersatzzug;

            if (ez.Zugnummer <= 0) {
                Logger.Warn(
                    "ÜB-Factory: <ersatzzug> ohne Zugnummer ignoriert (SEV-Zug={0}/{1})",
                    sev.ZugNr,
                    sev.Verkehrstag);
                continue;
            }

            var ezVt = ez.Verkehrstag == default
                ? sev.Verkehrstag.Value
                : ez.Verkehrstag;

            var ek = new UebZugKey(ez.Zugnummer, ezVt);

            // Nur anlegen, wenn kein <zug> existiert
            if (!zugMap.ContainsKey(ek)) {
                Logger.Info(
                    "ÜB-Factory: Ersatzzug ohne eigenes <zug> → Minimalzug: Zug={0}/{1}",
                    ez.Zugnummer,
                    ezVt);

                var ersatzZug = CreateZugFromErsatzzug(sev);
                ersatzZug.Zugnummer   = ez.Zugnummer;
                ersatzZug.Verkehrstag = ezVt;

                zugMap[ek]     = ersatzZug;
                regelungen[ek] = [];
            }
            else {
                Logger.Debug(
                    "ÜB-Factory: Ersatzzug später als <zug> definiert → kein Duplikat: Zug={0}/{1}",
                    ez.Zugnummer,
                    ezVt);
            }
        }

        Logger.Info(
            "ÜB-Factory abgeschlossen: Zuege={0}, RegelungsKeys={1}",
            zugMap.Count,
            regelungen.Count);

        return new UebZugFactoryResult {
            Zuege      = zugMap.Values.ToList(),
            Regelungen = regelungen
        };
    }

    // =====================================================================
    // Regelungen
    // =====================================================================
    private static UebRegelungDto CreateKomplettausfallRegelung(UebZugDto zug) =>
        new() {
            Art         = "Ausfall",
            AnchorRl100 = zug.Regelweg?.Abgangsbahnhof?.Ds100
        };

    private static UebRegelungDto CreateTeilAusfallRegelung(UebSevDto sev) =>
        new() {
            Art         = "Teilausfall",
            AnchorRl100 = sev.AusfallVonDs100,
            JsonRaw     = JsonSerializer.Serialize(sev)
        };


    // =====================================================================
    // Zug-Erzeugung
    // =====================================================================

    private static UebZugDto CreateZugFromSev(UebSevDto sev) =>
        new() {
            Zugnummer     = sev.ZugNr,
            Verkehrstag   = sev.Verkehrstag!.Value,
            FploAbschnitt = "SEV",
            Regelweg = new ZvFExportBaumassnahmenBaumassnahmeZuegeZugRegelweg {
                Abgangsbahnhof = new BetriebsstelleDS100 { Ds100 = sev.StartDs100 },
                Zielbahnhof    = new BetriebsstelleDS100 { Ds100 = sev.EndDs100 }
            }
        };

    private static UebZugDto CreateZugFromErsatzzug(UebSevDto sev) {
        var ez = sev.Ersatzzug!;

        return new UebZugDto {
            FploAbschnitt = "Ersatzzug",
            Regelweg = new ZvFExportBaumassnahmenBaumassnahmeZuegeZugRegelweg {
                Abgangsbahnhof = new BetriebsstelleDS100 { Ds100 = ez.AbgangDs100 },
                Zielbahnhof    = new BetriebsstelleDS100 { Ds100 = ez.ZielDs100 }
            }
        };
    }


    private static void AddOrMergeRegelung(
        List<UebRegelungDto> list,
        UebRegelungDto       neu,
        UebZugKey            key) {
        if (neu == null || string.IsNullOrWhiteSpace(neu.AnchorRl100))
            return;

        if (list.Any(r =>
                r.Art         == neu.Art &&
                r.AnchorRl100 == neu.AnchorRl100)) {
            Logger.Debug(
                "ÜB-Factory: Doppelte Regelung ignoriert: Zug={0}/{1}, Art={2}, Anker={3}",
                key.ZugNr,
                key.Verkehrstag,
                neu.Art,
                neu.AnchorRl100);
            return;
        }

        list.Add(neu);
    }

    private static bool IsKomplettausfall(UebZugDto zug) =>
        string.Equals(zug.FploAbschnitt, "Ausfall", StringComparison.OrdinalIgnoreCase);
}