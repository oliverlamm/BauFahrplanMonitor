namespace BauFahrplanMonitor.Importer.Dto.BbpNeo;

/// <summary>
/// RAW-DTO für eine einzelne IAV-Betroffenheit
/// (Industrieanschluss-Verkehr) aus dem BBPNeo-XML.
///
/// Enthält die unverarbeiteten, textuellen Werte
/// direkt aus der XML-Struktur.
/// </summary>
/// <remarks>
/// Dieses DTO ist:
///  - Ergebnis des Streaming-Parsers
///  - vollständig XML-nah
///  - bewusst ohne Typkonvertierung oder Validierung
///
/// Zweck:
/// <list type="bullet">
///   <item>verlustfreie Übernahme aller IAV-Daten</item>
///   <item>Eingabe für den <see cref="BbpNeoNormalizer"/></item>
/// </list>
///
/// Architekturrolle:
/// <code>
/// XML
///  ↓
/// BbpNeoIavBetroffenheitRaw   ← HIER (RAW)
///  ↓
/// BbpNeoNormalizer
///  ↓
/// BbpNeoIavBetroffenheit      (Domain)
/// </code>
///
/// In dieser Klasse darf **keine fachliche Logik**
/// implementiert werden.
/// </remarks>
public sealed class BbpNeoIavBetroffenheitRaw {

    // ==========================================================
    // LOKALISIERUNG / REFERENZEN (RAW)
    // ==========================================================

    /// <summary>
    /// Betriebsstelle des Industrieanschlusses (RAW).
    /// </summary>
    /// <remarks>
    /// Enthält typischerweise einen DS100- oder Ril100-Code
    /// und wird im Normalizer bereinigt und aufgelöst.
    /// </remarks>
    public string? Betriebsstelle { get; set; }

    /// <summary>
    /// VzG-Streckennummer des Industrieanschlusses (RAW).
    /// </summary>
    public string? VzGStrecke { get; set; }

    /// <summary>
    /// Anschlussgrenze des Industrieanschlusses (RAW).
    /// </summary>
    public string? Anschlussgrenze { get; set; }

    // ==========================================================
    // VERTRAGSDATEN (RAW)
    // ==========================================================

    /// <summary>
    /// Vertragsnummer des Industrieanschlusses (RAW).
    /// </summary>
    /// <remarks>
    /// Wird im Normalizer als Pflichtfeld geprüft
    /// und als fachlicher Schlüssel verwendet.
    /// </remarks>
    public string? VertragNr { get; set; }

    /// <summary>
    /// Art des Vertrags (RAW).
    /// </summary>
    public string? VertragArt { get; set; }

    /// <summary>
    /// Status des Vertrags (RAW).
    /// </summary>
    public string? VertragStatus { get; set; }

    /// <summary>
    /// Kunde des Industrieanschlusses (RAW).
    /// </summary>
    public string? Kunde { get; set; }

    // ==========================================================
    // TECHNISCHE MERKMALE (RAW)
    // ==========================================================

    /// <summary>
    /// Kennzeichnung, ob die Oberleitung betroffen ist (RAW).
    /// </summary>
    /// <remarks>
    /// Typische Werte:
    ///  - "0" / "1"
    ///  - "true" / "false"
    ///  - "ja" / "nein"
    /// </remarks>
    public string? Oberleitung { get; set; }

    /// <summary>
    /// Kennzeichnung, ob die Oberleitung abgeschaltet ist (RAW).
    /// </summary>
    public string? OberleitungAus { get; set; }

    // ==========================================================
    // BETRIEBLICHE AUSWIRKUNGEN (RAW)
    // ==========================================================

    /// <summary>
    /// Einschränkung der Bedienbarkeit des Industrieanschlusses (RAW).
    /// </summary>
    public string? EinschraenkungBedienbarkeitIA { get; set; }

    /// <summary>
    /// Freitext-Kommentar zur IAV-Betroffenheit (RAW).
    /// </summary>
    public string? Kommentar { get; set; }
}
