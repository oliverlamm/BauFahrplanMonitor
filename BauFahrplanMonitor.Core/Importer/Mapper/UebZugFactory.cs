using System.Text.Json;
using System.Text.Json.Serialization;
using BauFahrplanMonitor.Core.Importer.Dto.Ueb;
using BauFahrplanMonitor.Core.Importer.Helper;
using BauFahrplanMonitor.Core.Importer.Xml;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace BauFahrplanMonitor.Core.Importer.Mapper;

public static class UebZugFactory {
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static UebZugFactoryResult Build(UebDocumentDto document) {
        var zugMap =
            new Dictionary<UebZugKey, UebZugDto>();

        var regelungen =
            new Dictionary<UebZugKey,
                Dictionary<UebRegelungsKey, UebRegelungDto>>();

        // -------------------------------------------------
        // 1️⃣ ZÜGE aus <zug>
        // -------------------------------------------------
        foreach (var zug in document.Zuege) {
            var key = new UebZugKey(zug.Zugnummer, zug.Verkehrstag);

            zugMap[key]     = zug;
            regelungen[key] = new Dictionary<UebRegelungsKey, UebRegelungDto>();

            AddBasisFploAbschnittRegelung(
                regelungen, key, zug);
        }

        // -------------------------------------------------
        // 2️⃣ SEV / Ersatzzüge
        // -------------------------------------------------
        foreach (var sev in document.Sev) {
            if (sev.Verkehrstag is null)
                continue;

            var key = new UebZugKey(sev.ZugNr, sev.Verkehrstag.Value);

            if (!zugMap.TryGetValue(key, out var zug)) {
                zug             = CreateZugFromSev(sev);
                zugMap[key]     = zug;
                regelungen[key] = new Dictionary<UebRegelungsKey, UebRegelungDto>();

                AddBasisFploAbschnittRegelung(
                    regelungen, key, zug);
            }

            // ➕ SEV-Regelung (Teilausfall)
            AddRegelung(
                regelungen,
                key,
                CreateSevDetailRegelung(sev));

            // -----------------------------
            // Ersatzzug
            // -----------------------------
            if (sev.Ersatzzug is not { } ez)
                continue;

            var ezKey = new UebZugKey(
                ez.Zugnummer,
                ez.Verkehrstag);

            if (zugMap.ContainsKey(ezKey))
                continue;

            var ersatzZug = CreateZugFromErsatzzug(sev);

            zugMap[ezKey]     = ersatzZug;
            regelungen[ezKey] = new Dictionary<UebRegelungsKey, UebRegelungDto>();

            AddBasisFploAbschnittRegelung(
                regelungen, ezKey, ersatzZug);
        }

        // -------------------------------------------------
        // DEBUG
        // -------------------------------------------------
        if (Logger.IsDebugEnabled) {
            Logger.Debug(
                "ÜB-Factory RESULT\n{Json}",
                ToDebugJson(new {
                    Zuege = zugMap.Values,
                    Regelungen = regelungen.ToDictionary(
                        z => $"{z.Key.ZugNr}/{z.Key.Verkehrstag}",
                        z => z.Value.Values)
                }));
        }

        return new UebZugFactoryResult {
            Zuege = zugMap.Values.ToList(),
            Regelungen = regelungen.ToDictionary(
                x => x.Key,
                x => x.Value.Values.ToList())
        };
    }

    // =====================================================================
    // Regelungen
    // =====================================================================

    private static void AddBasisFploAbschnittRegelung(
        Dictionary<UebZugKey,
            Dictionary<UebRegelungsKey, UebRegelungDto>> map,
        UebZugKey zugKey,
        UebZugDto zug) {
        var anchor = zug.FploAbschnitt switch {
            "Ausfall" => zug.Regelweg?.Abgangsbahnhof?.Ds100,

            _ => zug.Knotenzeiten?
                .OrderBy(k => k.RelativLage)
                .FirstOrDefault()
                ?.BahnhofDs100
        };

        if (string.IsNullOrWhiteSpace(anchor))
            return;

        AddRegelung(
            map,
            zugKey,
            new UebRegelungDto {
                Art         = zug.FploAbschnitt,
                AnchorRl100 = anchor,
                JsonRaw     = NormalizeJson(string.Empty)
            });
    }

    private static UebRegelungDto CreateSevDetailRegelung(UebSevDto sev) =>
        new() {
            Art         = "SEV",
            AnchorRl100 = sev.AusfallVonDs100,
            BisRl100    = sev.EndDs100,
            JsonRaw     = NormalizeJson(JsonSerializer.Serialize(sev))
        };

    private static void AddRegelung(
        Dictionary<UebZugKey,
            Dictionary<UebRegelungsKey, UebRegelungDto>> map,
        UebZugKey      zugKey,
        UebRegelungDto dto) {
        var rKey = new UebRegelungsKey(
            dto.Art,
            dto.AnchorRl100);

        map[zugKey][rKey] = dto; // 🔑 erzwingt Eindeutigkeit
    }

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
}