namespace BauFahrplanMonitor.Importer.Dto.BbpNeo;

/// <summary>
/// RAW-DTO für eine BVE (Betriebliche Verfahrensanordnung)
/// aus dem BBPNeo-XML.
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
///   <item>verlustfreie Übernahme aller BVE-Daten</item>
///   <item>Eingabe für den <see cref="BbpNeoNormalizer"/></item>
/// </list>
///
/// Architekturrolle:
/// <code>
/// XML
///  ↓
/// BbpNeoBveRaw          ← HIER (RAW)
///  ↓
/// BbpNeoNormalizer
///  ↓
/// BbpNeoBve             (Domain)
/// </code>
/// </remarks>
public sealed class BbpNeoBveRaw {

    // ==========================================================
    // IDENTITÄT (RAW)
    // ==========================================================

    /// <summary>
    /// Eindeutige BVE-ID aus dem XML.
    /// </summary>
    public string? BveId { get; set; }

    /// <summary>
    /// Kennzeichnung, ob die BVE aktiv ist (RAW).
    /// </summary>
    /// <remarks>
    /// Typische Werte:
    ///  - "0" / "1"
    ///  - "true" / "false"
    /// </remarks>
    public string? Aktiv { get; set; }

    // ==========================================================
    // ART / KLASSIFIKATION
    // ==========================================================

    /// <summary>
    /// Textuelle Beschreibung der Art der BVE (RAW).
    /// </summary>
    public string? ArtText { get; set; }

    // ==========================================================
    // STRECKENBEZUG (RAW)
    // ==========================================================

    /// <summary>
    /// Ril100-/DS100-Code der Abgangs-Betriebsstelle (RAW).
    /// </summary>
    public string? BstVonRil100 { get; set; }

    /// <summary>
    /// Ril100-/DS100-Code der Ziel-Betriebsstelle (RAW).
    /// </summary>
    public string? BstBisRil100 { get; set; }

    /// <summary>
    /// VzG-Nummer der Startstrecke (RAW).
    /// </summary>
    public string? VzGStrecke { get; set; }

    /// <summary>
    /// VzG-Nummer der Zielstrecke (RAW).
    /// </summary>
    public string? VzGStreckeBis { get; set; }

    // ==========================================================
    // ZEITLICHE GÜLTIGKEIT (RAW)
    // ==========================================================

    /// <summary>
    /// Textuelle Gültigkeitsbeschreibung (RAW).
    /// </summary>
    public string? Gueltigkeit { get; set; }

    /// <summary>
    /// Startdatum der Gültigkeit (RAW).
    /// </summary>
    public string? TagVon { get; set; }

    /// <summary>
    /// Startzeit der Gültigkeit (RAW).
    /// </summary>
    public string? ZeitVon { get; set; }

    /// <summary>
    /// Enddatum der Gültigkeit (RAW).
    /// </summary>
    public string? TagBis { get; set; }

    /// <summary>
    /// Endzeit der Gültigkeit (RAW).
    /// </summary>
    public string? ZeitBis { get; set; }

    /// <summary>
    /// Effektive Verkehrstage der Gültigkeit (RAW).
    /// </summary>
    public string? EffektiveVerkehrstage { get; set; }

    // ==========================================================
    // TEXTFELDER
    // ==========================================================

    /// <summary>
    /// Mikroskopische Ortsbeschreibung (RAW).
    /// </summary>
    public string? OrtMikroskop { get; set; }

    /// <summary>
    /// Freitext-Bemerkung zur BVE (RAW).
    /// </summary>
    public string? Bemerkung { get; set; }

    // ==========================================================
    // OPTIONALE CHILD-STRUKTUREN (RAW)
    // ==========================================================

    /// <summary>
    /// Industrieanschluss-Verkehre (IAV) – RAW.
    /// </summary>
    public BbpNeoIavRaw? Iav { get; set; }

    /// <summary>
    /// Anlagenbezogene Sperrungen (APS) – RAW.
    /// </summary>
    public BbpNeoApsRaw? Aps { get; set; }
}
