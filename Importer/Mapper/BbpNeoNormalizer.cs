using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BauFahrplanMonitor.Importer.Dto.BbpNeo;

namespace BauFahrplanMonitor.Importer.Mapper;

/// <summary>
/// Normalizer für BBPNeo-RAW-Daten.
///
/// Verantwortlich für:
///  - Umwandlung von RAW-DTOs in valide Domain-Objekte
///  - Typkonvertierungen (Datum, Bool, Nummern)
///  - Strukturierung verschachtelter Inhalte (Regelung → BVE → APS/IAV)
///  - Sammlung von Warnungen bei fehlerhaften oder unvollständigen Daten
///
/// NICHT verantwortlich für:
///  - XML-Parsing
///  - Datenbankzugriffe
///  - Referenzauflösung
///
/// Der Normalizer ist bewusst:
///  - fehlertolerant (Warnungen statt Exceptions)
///  - deterministisch
///  - frei von Seiteneffekten
/// </summary>
/// <remarks>
/// Architekturrolle:
///
///   Raw XML
///     ↓
///   BbpNeoRawXmlParser
///     ↓
///   *Raw DTOs*
///     ↓
///   BbpNeoNormalizer          ← HIER
///     ↓
///   Domain-Objekte
///     ↓
///   BbpNeoUpsertService
///
/// Der Normalizer bildet die **einzige Stelle**,
/// an der fachliche Interpretation stattfindet.
/// </remarks>
public static class BbpNeoNormalizer {

    // =====================================================================
    // ENTRY
    // =====================================================================
    /// <summary>
    /// Normalisiert eine BBPNeo-RAW-Maßnahme zu einem Domain-Objekt.
    /// </summary>
    /// <param name="raw">RAW-Maßnahme aus dem XML-Parser</param>
    /// <returns>
    /// <see cref="NormalizationResult{T}"/> mit:
    ///  - <c>Value</c>: normalisierte Maßnahme
    ///  - <c>Warnings</c>: nicht-fatalen Problemen
    /// </returns>
    /// <remarks>
    /// Harte Abbruchbedingung:
    ///  - fehlende MasId → Exception
    ///
    /// Alle anderen Probleme werden als Warnungen gesammelt,
    /// damit der Import möglichst vollständig fortgesetzt werden kann.
    /// </remarks>
    public static NormalizationResult<BbpNeoMassnahme>
        Normalize(BbpNeoMassnahmeRaw raw) {

        var warnings = new List<string>();

        if( string.IsNullOrWhiteSpace(raw.MasId) )
            warnings.Add("MasId fehlt");

        // ---------------------------------------------------------
        // Regelungen normalisieren
        // ---------------------------------------------------------
        var regelungen = raw.Regelungen
            .Select(r => NormalizeRegelung(r, warnings))
            .Where(r => r != null)
            .ToList();

        // ---------------------------------------------------------
        // Maßnahme zusammensetzen
        // ---------------------------------------------------------
        var massnahme = new BbpNeoMassnahme {
            MasId = raw.MasId
                    ?? throw new Exception("MasId darf nicht leer sein, Abbruch"),
            Aktiv = ParseBool(raw.Aktiv),
            Beginn = ParseDateTime(raw.MasBeginn, "MasBeginn", warnings),
            Ende = ParseDateTime(raw.MasEnde, "MasEnde", warnings),
            Anmeldung = ParseDateOnly(raw.Anmeldung,"Anmeldung", warnings),
            Genehmigung = raw.Genehmigung,
            AnforderungBbzr = ParseDateOnly(raw.DatumAuftragBBZR,"AnforderungBbzr",warnings),
            ArtDerArbeit = raw.ArtDerArbeiten ?? string.Empty,
            Arbeiten = raw.Arbeiten ?? string.Empty,
            RegionRef = ParseLong(raw.RegionId),
            MasVonBstDs100 = raw.MasBstVonRil100,
            MasBisBstDs100 = raw.MasBstBisRil100,
            MasVonVzG = ParseLong(raw.VzGStrecke),
            MasBisVzG = ParseLong(raw.VzGStreckeBis),
            MasVonKmL = raw.MasKmVon,
            MasBisKmL = raw.MasKmBis,
            Regelungen = regelungen
                .Where(r => r is not null)
                .Select(r => r!)
                .ToList()
        };

        return new NormalizationResult<BbpNeoMassnahme> {
            Value = massnahme,
            Warnings = warnings
        };
    }

    // =====================================================================
    // REGELUNG
    // =====================================================================

    /// <summary>
    /// Normalisiert eine RAW-Regelung.
    /// </summary>
    private static BbpNeoRegelung? NormalizeRegelung(
        BbpNeoRegelungRaw raw,
        List<string> warnings) {

        if( string.IsNullOrWhiteSpace(raw.RegId) ) {
            warnings.Add("Regelung ohne RegId verworfen");
            return null;
        }

        var bven = raw.Bven
            .Select(b => NormalizeBve(b, warnings))
            .Where(b => b != null)
            .ToList();

        return new BbpNeoRegelung {
            RegId = raw.RegId!,
            Aktiv = ParseBool(raw.Aktiv),

            Beginn = ParseDateTime(raw.Beginn, "Regelung.Beginn", warnings),
            Ende = ParseDateTime(raw.Ende, "Regelung.Ende", warnings),

            Zeitraum = raw.Zeitraum,
            Richtung = ParseShort(raw.Richtung) ?? 0,

            BplArt = raw.BplArtText,
            RegelungKurz = raw.BplRegelungKurz,
            RegelungLang = raw.BplRegelungLang,

            Durchgehend = ParseBool(raw.Durchgehend),
            Schichtweise = ParseBool(raw.Schichtweise),
            
            VonBstDs100 = raw.BstVonRil100,
            BisBstDs100 = raw.BstBisRil100,

            VonVzG = ParseLong(raw.VzGStrecke),
            BisVzG = ParseLong(raw.VzGStreckeBis),

            Bven = bven
            .Where(r => r is not null)
            .Select(r => r!)
            .ToList()
        };
    }

    // =====================================================================
    // BVE
    // =====================================================================

    /// <summary>
    /// Normalisiert eine RAW-BVE.
    /// </summary>
    private static BbpNeoBve? NormalizeBve(
        BbpNeoBveRaw raw,
        List<string> warnings) {

        if( string.IsNullOrWhiteSpace(raw.BveId) ) {
            warnings.Add("BVE ohne BveId verworfen");
            return null;
        }

        var von = Combine(raw.TagVon, raw.ZeitVon, warnings, "BVE.Von");
        var bis = Combine(raw.TagBis, raw.ZeitBis, warnings, "BVE.Bis");

        return new BbpNeoBve {
            BveId = raw.BveId!,
            Aktiv = ParseBool(raw.Aktiv),

            Art = raw.ArtText,
            OrtMikroskopisch = raw.OrtMikroskop,
            Bemerkung = raw.Bemerkung ?? string.Empty,

            Gueltigkeit = raw.Gueltigkeit,
            GueltigkeitEffektiveVerkehrstage =
                raw.EffektiveVerkehrstage,

            GueltigkeitVon = von,
            GueltigkeitBis = bis,

            VonBstDs100 = raw.BstVonRil100,
            BisBstDs100 = raw.BstBisRil100,
            VonVzG = ParseLong(raw.VzGStrecke),
            BisVzG = ParseLong(raw.VzGStreckeBis),

            // 🔹 HEADER + DETAILS
            Aps = raw.Aps != null
                ? NormalizeAps(raw.Aps)
                : null,

            Iav = raw.Iav != null
                ? NormalizeIav(raw.Iav)
                : null
        };
    }

    // =====================================================================
    // IAV
    // =====================================================================

    /// <summary>
    /// Normalisiert eine IAV-Struktur.
    /// </summary>
    private static BbpNeoIav NormalizeIav(
        BbpNeoIavRaw raw) {

        return new BbpNeoIav {
            Betroffenheit = ParseBool(raw.Betroffenheit),
            Beschreibung = raw.Beschreibung ?? string.Empty,

            Betroffenheiten = raw.Betroffenheiten
                .Where(b => !string.IsNullOrWhiteSpace(b.VertragNr))
                .Select(b => new BbpNeoIavBetroffenheit {
                    VertragNr = b.VertragNr!,
                    Kunde = b.Kunde,
                    Anschlussgrenze = b.Anschlussgrenze,
                    VertragArt = b.VertragArt,
                    VertragStatus = b.VertragStatus,
                    Oberleitung = ParseBool(b.Oberleitung),
                    OberleitungAus = ParseBool(b.OberleitungAus),
                    EinschraenkungBedienbarkeitIA =
                        b.EinschraenkungBedienbarkeitIA,
                    Kommentar = b.Kommentar,
                    BstDs100 = b.Betriebsstelle,
                    VzgStrecke = ParseLong(b.VzGStrecke)
                })
                .ToList()
        };
    }

    // =====================================================================
    // APS
    // =====================================================================

    /// <summary>
    /// Normalisiert eine APS-Struktur.
    /// </summary>
    private static BbpNeoAps NormalizeAps(
        BbpNeoApsRaw raw) {

        return new BbpNeoAps {
            Betroffenheit = ParseBool(raw.Betroffenheit),
            Beschreibung = raw.Beschreibung ?? string.Empty,
            FreiVonFahrzeugen = ParseBool(raw.FreiVonFahrzeugen),

            Betroffenheiten = raw.Betroffenheiten
                .Where(b => !string.IsNullOrWhiteSpace(b.Uuid))
                .Select(b => new BbpNeoApsBetroffenheit {
                    Uuid = b.Uuid!,
                    BstDs100 = b.Ds100,
                    Gleis = b.GleisNr,
                    PrimaereKat = b.PrimaereKategorie,
                    SekundaerKat = b.SekundaereKategorie,
                    Oberleitung = ParseBool(b.Oberleitung),
                    OberleitungAus = ParseBool(b.OberleitungAus),
                    TechnischerPlatz = b.TechnischerPlatz,
                    ArtDerAnbindung = b.ArtDerAnbindung,
                    EinschraenkungBefahrbarkeitSe =
                        b.EinschraenkungBefahrbarkeitSE,
                    Kommentar = b.Kommentar,
                    MoeglicheZAs = b.MoeglicheZA,
                    AbFahrplanjahr = ParseShort(b.AbFahrplanjahr)
                })
                .ToList()
        };
    }

    // =====================================================================
    // HELPER (konsequent zentralisiert)
    // =====================================================================

    /// <summary>
    /// Robuste Bool-Konvertierung aus Textwerten.
    /// </summary>
    private static bool ParseBool(string? raw) {
        if( string.IsNullOrWhiteSpace(raw) )
            return false;

        return raw switch {
            "1"                                                            => true,
            "0"                                                            => false,
            _ when raw.Equals("true", StringComparison.OrdinalIgnoreCase)  => true,
            _ when raw.Equals("false", StringComparison.OrdinalIgnoreCase) => false,
            _ when raw.Equals("ja", StringComparison.OrdinalIgnoreCase)    => true,
            _                                                              => false
        };
    }

    /// <summary>
    /// Long-Konvertierung mit InvariantCulture.
    /// </summary>
    private static long? ParseLong(string? raw) {
        if( string.IsNullOrWhiteSpace(raw) )
            return null;

        return long.TryParse(
            raw,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var v)
            ? v
            : null;
    }

    /// <summary>
    /// DateTime-Parsing (yyyyMMddHHmm).
    /// </summary>
    private static DateTime? ParseDateTime(
        string? raw,
        string field,
        List<string> warnings) {

        if( string.IsNullOrWhiteSpace(raw) )
            return null;

        if( DateTime.TryParseExact(
            raw,
            "yyyyMMddHHmm",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var dt) )
            return dt;

        warnings.Add($"Ungültiges DateTime ({field}): '{raw}'");
        return null;
    }

    /// <summary>
    /// Kombiniert getrennte Datums- und Zeitfelder
    /// zu einem DateTime.
    /// </summary>
    private static DateTime? Combine(
        string? rawDate,
        string? rawTime,
        List<string> warnings,
        string field) {

        if( string.IsNullOrWhiteSpace(rawDate) )
            return null;

        DateOnly date;

        if( !DateOnly.TryParseExact(
            rawDate,
            [
                "yyyyMMdd",
                "dd.MM.yyyy"
            ],
            CultureInfo.GetCultureInfo("de-DE"),
            DateTimeStyles.None,
            out date) ) {

            warnings.Add($"Ungültiges Datum ({field}): '{rawDate}'");
            return null;
        }

        var time = TimeOnly.MinValue;

        if (string.IsNullOrWhiteSpace(rawTime) ||
            TimeOnly.TryParseExact(
                rawTime,
                [
                    "HHmm",
                    "HH:mm:ss"
                ],
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out time)) return date.ToDateTime(time);
        warnings.Add($"Ungültige Zeit ({field}): '{rawTime}'");
        time = TimeOnly.MinValue;

        return date.ToDateTime(time);
    }

    /// <summary>
    /// Short-Konvertierung.
    /// </summary>
    private static short? ParseShort(string? raw) {
        if( string.IsNullOrWhiteSpace(raw) )
            return null;

        if( short.TryParse(
            raw,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var v) )
            return v;

        return null;
    }
    
    /// <summary>
    /// DateOnly-Parsing aus BBPNeo-Formaten.
    /// </summary>
    private static DateOnly? ParseDateOnly(
        string? raw,
        string field,
        List<string> warnings) {

        if( string.IsNullOrWhiteSpace(raw) )
            return null;

        // akzeptiert yyyyMMddHHmm
        if( DateTime.TryParseExact(
            raw,
            "yyyyMMddHHmm",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var dt) )
            return DateOnly.FromDateTime(dt);

        warnings.Add($"Ungültiges DateOnly ({field}): '{raw}'");
        return null;
    }

}
