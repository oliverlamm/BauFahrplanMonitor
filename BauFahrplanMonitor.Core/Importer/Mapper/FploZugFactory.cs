using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using BauFahrplanMonitor.Importer.Dto.Fplo;
using BauFahrplanMonitor.Importer.Helper;
using BauFahrplanMonitor.Importer.Xml;
using NLog;

namespace BauFahrplanMonitor.Importer.Mapper;

public static class FploZugFactory {
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static FploZugFactoryResult Build(FploDocumentDto document) {
        var zugMap =
            new Dictionary<FploZugKey, FploZugDto>();

        var regelungen =
            new Dictionary<FploZugKey,
                Dictionary<FploRegelungsKey, FploRegelungDto>>();

        // -------------------------------------------------
        // 1️⃣ ZÜGE aus <zug>
        // -------------------------------------------------
        foreach (var zug in document.Zuege) {
            var key = new FploZugKey(zug.Zugnummer, zug.Verkehrstag);

            zugMap[key]     = zug;
            regelungen[key] = new Dictionary<FploRegelungsKey, FploRegelungDto>();

            AddBasisFploAbschnittRegelung(regelungen, key, zug);
        }

        // -------------------------------------------------
        // 2️⃣ SEV / Ersatzzüge
        // -------------------------------------------------
        foreach (var sev in document.Sev) {
            if (sev.Verkehrstag is null)
                continue;

            var key = new FploZugKey(sev.ZugNr, sev.Verkehrstag.Value);

            if (!zugMap.TryGetValue(key, out var zug)) {
                zug             = CreateZugFromSev(sev);
                zugMap[key]     = zug;
                regelungen[key] = new Dictionary<FploRegelungsKey, FploRegelungDto>();

                AddBasisFploAbschnittRegelung(regelungen, key, zug);
            }

            // ➕ SEV-Regelung (Teilausfall)
            AddRegelung(regelungen, key, CreateSevDetailRegelung(sev));

            // -----------------------------
            // Ersatzzug
            // -----------------------------
            if (sev.Ersatzzug is not { } ez)
                continue;

            var ezKey = new FploZugKey(ez.Zugnummer, ez.Verkehrstag);

            if (zugMap.ContainsKey(ezKey))
                continue;

            var ersatzZug = CreateZugFromErsatzzug(sev);

            zugMap[ezKey]     = ersatzZug;
            regelungen[ezKey] = new Dictionary<FploRegelungsKey, FploRegelungDto>();

            AddBasisFploAbschnittRegelung(regelungen, ezKey, ersatzZug);
        }

        // -------------------------------------------------
        // 3️⃣ Weitere Regelungen (FPLO)
        // -------------------------------------------------
        AddHaltausfallRegelungen(document, zugMap, regelungen);
        AddZurueckgehaltenRegelungen(document, zugMap, regelungen);
        AddZugparameterRegelungen(document, zugMap, regelungen);

        // -------------------------------------------------
        // DEBUG
        // -------------------------------------------------
        if (Logger.IsDebugEnabled) {
            Logger.Debug(
                "Fplo-Factory RESULT\n{Json}",
                ToDebugJson(new {
                    Zuege = zugMap.Values,
                    Regelungen = regelungen.ToDictionary(
                        z => $"{z.Key.ZugNr}/{z.Key.Verkehrstag}",
                        z => z.Value.Values)
                }));
        }

        return new FploZugFactoryResult {
            Zuege = zugMap.Values.ToList(),
            Regelungen = regelungen.ToDictionary(
                x => x.Key,
                x => x.Value.Values.ToList())
        };
    }

    // =====================================================================
    // Regelungen: Basis
    // =====================================================================
    private static void AddBasisFploAbschnittRegelung(
        Dictionary<FploZugKey, Dictionary<FploRegelungsKey, FploRegelungDto>> map,
        FploZugKey                                                            zugKey,
        FploZugDto                                                            zug) {
        
        if (zug.IstErsatzzug)
            return;
        
        // 1️⃣ XML → interner Fachtyp
        if (!TryParseFploAbschnitt(zug.FploAbschnitt, out var type)) {
            Logger.Warn(
                "Unbekannter fplo_abschnitt '{0}' – keine Regelung erzeugt (Zug={1}, Tag={2})",
                zug.FploAbschnitt,
                zug.Zugnummer,
                zug.Verkehrstag);
            return;
        }

        // 2️⃣ Fachtyp → DB-Art
        var art = ToDbRegelungsArt(type);

        // 3️⃣ Anker bestimmen
        var anchor =
            zug.Regelweg?.Abgangsbahnhof?.Ds100 ??
            zug.Regelweg?.Zielbahnhof?.Ds100;

        if (string.IsNullOrWhiteSpace(anchor)) {
            Logger.Info(
                "FPLO-Regelung '{0}' ohne Anker – übersprungen (Zug={1}, Tag={2})",
                art,
                zug.Zugnummer,
                zug.Verkehrstag);
            return;
        }

        if (type == FploAbschnittType.Ausfall) {
            var json = JsonSerializer.Serialize(
                new {
                    Art        = "Ausfall",
                    AusfallVon = zug.Regelweg?.Abgangsbahnhof?.Ds100,
                    AusfallBis = zug.Regelweg?.Zielbahnhof?.Ds100
                },
                new JsonSerializerOptions {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });

            AddRegelung(
                map,
                zugKey,
                new FploRegelungDto {
                    Art         = RegelungsArt.Ausfall,
                    AnchorRl100 = zug.Regelweg?.Abgangsbahnhof?.Ds100,
                    JsonRaw     = json
                });

            return;
        }
        
        // 4️⃣ Regelung anlegen (JsonRaw = null bei Abschnitt!)
        AddRegelung(
            map,
            zugKey,
            new FploRegelungDto {
                Art         = art,
                AnchorRl100 = anchor,
                JsonRaw     = null
            });
    }

    // =====================================================================
    // Regelung: Zugparameter
    // =====================================================================
    private static void AddZugparameterRegelungen(
        FploDocumentDto                                                       document,
        Dictionary<FploZugKey, FploZugDto>                                    zugMap,
        Dictionary<FploZugKey, Dictionary<FploRegelungsKey, FploRegelungDto>> regelungen) {
        
        foreach (var zp in document.Zugparameter) {
            if (zp.ZugNr is not { } zugNr)
                continue;

            if (zp.Verkehrstag is not { } verkehrstag)
                continue;

            var key = new FploZugKey(zugNr, verkehrstag);

            if (!zugMap.ContainsKey(key))
                continue;

            if (string.IsNullOrWhiteSpace(zp.WirktAbBstDs100))
                continue;

            AddRegelung(
                regelungen,
                key,
                new FploRegelungDto {
                    Art         = RegelungsArt.Zugparameter,
                    AnchorRl100 = zp.WirktAbBstDs100,
                    JsonRaw     = NormalizeJson(JsonSerializer.Serialize(zp))
                });
        }
    }

    // =====================================================================
    // Regelung: Zurückgehalten
    // =====================================================================
    private static void AddZurueckgehaltenRegelungen(
        FploDocumentDto                                                       document,
        Dictionary<FploZugKey, FploZugDto>                                    zugMap,
        Dictionary<FploZugKey, Dictionary<FploRegelungsKey, FploRegelungDto>> regelungen) {
        
        foreach (var zg in document.Zurueckgehalten) {
            if (zg.ZugNr is not { } zugNr)
                continue;

            if (zg.Verkehrstag is not { } verkehrstag)
                continue;

            var key = new FploZugKey(zugNr, verkehrstag);

            if (!zugMap.ContainsKey(key))
                continue;

            if (string.IsNullOrWhiteSpace(zg.AbBstDs100))
                continue;

            AddRegelung(
                regelungen,
                key,
                new FploRegelungDto {
                    Art         = RegelungsArt.Zurueckgehalten,
                    AnchorRl100 = zg.AbBstDs100,
                    JsonRaw     = NormalizeJson(JsonSerializer.Serialize(zg))
                });
        }
    }

    // =====================================================================
    // Regelung: Haltausfall
    // =====================================================================
    private static void AddHaltausfallRegelungen(
        FploDocumentDto                                                       document,
        Dictionary<FploZugKey, FploZugDto>                                    zugMap,
        Dictionary<FploZugKey, Dictionary<FploRegelungsKey, FploRegelungDto>> regelungen) {
        
        foreach (var ha in document.Haltausfall) {
            if (ha.ZugNr is not { } zugNr)
                continue;

            if (ha.Verkehrstag is not { } verkehrstag)
                continue;

            var key = new FploZugKey(zugNr, verkehrstag);

            if (!zugMap.ContainsKey(key))
                continue;

            var anchor =
                ha.AusfallenderHaltDs100 ??
                ha.ErsatzHaltDs100;

            if (string.IsNullOrWhiteSpace(anchor))
                continue;

            AddRegelung(
                regelungen,
                key,
                new FploRegelungDto {
                    Art         = RegelungsArt.Haltausfall,
                    AnchorRl100 = anchor,
                    JsonRaw     = NormalizeJson(JsonSerializer.Serialize(ha))
                });
        }
    }
    
    // =====================================================================
    // Regelung: SEV
    // =====================================================================
    private static FploRegelungDto CreateSevDetailRegelung(FploSevDto sev) =>
        new() {
            Art         = RegelungsArt.Sev, // ✅
            AnchorRl100 = sev.AusfallVonDs100,
            JsonRaw     = NormalizeJson(JsonSerializer.Serialize(sev))
        };
    
    // =====================================================================
    // Zug-Erzeugung
    // =====================================================================
    private static FploZugDto CreateZugFromSev(FploSevDto sev) =>
        new() {
            Zugnummer     = sev.ZugNr,
            Verkehrstag   = sev.Verkehrstag!.Value,
            FploAbschnitt = "SEV",
            Regelweg = new ZvFExportBaumassnahmenBaumassnahmeZuegeZugRegelweg {
                Abgangsbahnhof = new BetriebsstelleDS100 { Ds100 = sev.StartDs100 },
                Zielbahnhof    = new BetriebsstelleDS100 { Ds100 = sev.EndDs100 }
            }
        };

    private static FploZugDto CreateZugFromErsatzzug(FploSevDto sev) {
        var ez = sev.Ersatzzug!;

        return new FploZugDto {
            IstErsatzzug = true,
            Zugnummer   = ez.Zugnummer,
            Verkehrstag = ez.Verkehrstag!,

            FploAbschnitt = "Ersatzzug",
            Regelweg = new ZvFExportBaumassnahmenBaumassnahmeZuegeZugRegelweg {
                Abgangsbahnhof = new BetriebsstelleDS100 { Ds100 = ez.AbgangDs100 },
                Zielbahnhof    = new BetriebsstelleDS100 { Ds100 = ez.ZielDs100 }
            }
        };
    }


    private static string? NormalizeJson(string? json) {
        return string.IsNullOrWhiteSpace(json) ? null : json;
    }


    private static string ToDebugJson(object obj) =>
        JsonSerializer.Serialize(
            obj,
#pragma warning disable CA1869
            new JsonSerializerOptions {
#pragma warning restore CA1869
                WriteIndented          = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder                = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

    private static readonly HashSet<string> RegelungsartenMitJson = new() {
        RegelungsArt.Ausfall,
        RegelungsArt.Sev, // ✅ SEV braucht Json
        RegelungsArt.Haltausfall,
        RegelungsArt.Zurueckgehalten,
        RegelungsArt.Zugparameter
    };

    private static void ValidateJsonUsage(string regelungsArt, string? jsonRaw) {
        if (jsonRaw == null && RegelungsartenMitJson.Contains(regelungsArt))
            throw new InvalidOperationException(
                $"Regelungsart '{regelungsArt}' erfordert JsonRaw, ist aber NULL.");

        if (jsonRaw != null && !RegelungsartenMitJson.Contains(regelungsArt))
            throw new InvalidOperationException(
                $"Regelungsart '{regelungsArt}' darf KEIN JsonRaw haben.");
    }

    private static bool TryParseFploAbschnitt(
        string?               raw,
        out FploAbschnittType type) {
        type = FploAbschnittType.Unbekannt;

        if (string.IsNullOrWhiteSpace(raw))
            return false;

        switch (raw.Trim()) {
            case "Umleitung":
                type = FploAbschnittType.Umleitung;
                return true;

            case "Zusätzliche Leistung":
            case "Zusätzliche Leistungen":
                type = FploAbschnittType.ZusaetzlicheLeistung;
                return true;

            case "Vorplanfahrt":
                type = FploAbschnittType.Vorplanfahrt;
                return true;

            case "Verspätung auf Regelweg":
                type = FploAbschnittType.VerspaetungRegelweg;
                return true;

            case "Ausfall":
                type = FploAbschnittType.Ausfall;
                return true;

            case "SEV":
                type = FploAbschnittType.SevAbschnitt;
                return true;

            default:
                return false;
        }
    }

    private static string ToDbRegelungsArt(FploAbschnittType type) {
        return type switch {
            FploAbschnittType.Umleitung            => RegelungsArt.Umleitung,
            FploAbschnittType.ZusaetzlicheLeistung => RegelungsArt.ZusaetzlicheLeistung,
            FploAbschnittType.Vorplanfahrt         => RegelungsArt.Vorplanfahrt,
            FploAbschnittType.VerspaetungRegelweg  => RegelungsArt.VerspaetungRegelweg,
            FploAbschnittType.Ausfall              => RegelungsArt.Ausfall,
            FploAbschnittType.SevAbschnitt         => RegelungsArt.SevAbschnitt,
            _ => throw new InvalidOperationException(
                $"Kein DB-Mapping für AbschnittType '{type}'")
        };
    }
    
    private static void AddRegelung(
        Dictionary<FploZugKey, Dictionary<FploRegelungsKey, FploRegelungDto>> map,
        FploZugKey                                                            zugKey,
        FploRegelungDto                                                       regelung) {
        ValidateJsonUsage(regelung.Art, regelung.JsonRaw);

        if (!map.TryGetValue(zugKey, out var regs)) {
            regs        = new Dictionary<FploRegelungsKey, FploRegelungDto>();
            map[zugKey] = regs;
        }

        var key = new FploRegelungsKey(
            regelung.Art,
            regelung.AnchorRl100);

        regs.TryAdd(key, regelung);
    }
    
}