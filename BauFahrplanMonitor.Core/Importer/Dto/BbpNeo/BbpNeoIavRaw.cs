namespace BauFahrplanMonitor.Core.Importer.Dto.BbpNeo;

/// <summary>
/// RAW-DTO für IAV (Industrieanschluss-Verkehre)
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
///   <item>verlustfreie Übernahme aller IAV-Informationen</item>
///   <item>Eingabe für den <see cref="BbpNeoNormalizer"/></item>
/// </list>
///
/// Architekturrolle:
/// <code>
/// XML
///  ↓
/// BbpNeoIavRaw          ← HIER (RAW)
///  ↓
/// BbpNeoNormalizer
///  ↓
/// BbpNeoIav             (Domain)
/// </code>
///
/// In dieser Klasse darf **keine fachliche Logik**
/// implementiert werden.
/// </remarks>
public sealed class BbpNeoIavRaw {

    // ==========================================================
    // KOPF-INFORMATIONEN (RAW)
    // ==========================================================

    /// <summary>
    /// Kennzeichnung, ob Industrieanschlüsse betroffen sind (RAW).
    /// </summary>
    /// <remarks>
    /// Typische Werte im XML:
    ///  - "0" / "1"
    ///  - "true" / "false"
    ///  - "ja" / "nein"
    ///
    /// Die Interpretation erfolgt ausschließlich
    /// im Normalizer.
    /// </remarks>
    public string? Betroffenheit { get; set; }

    /// <summary>
    /// Freitext-Beschreibung der IAV-Betroffenheit (RAW).
    /// </summary>
    public string? Beschreibung { get; set; }

    // ==========================================================
    // DETAILSTRUKTUREN (RAW)
    // ==========================================================

    /// <summary>
    /// Liste der einzelnen betroffenen Industrieanschlüsse (RAW).
    /// </summary>
    /// <remarks>
    /// Enthält eine oder mehrere
    /// <see cref="BbpNeoIavBetroffenheitRaw"/>-Einträge,
    /// die die Betroffenheit im Detail beschreiben.
    ///
    /// Die Liste ist:
    ///  - leer, wenn keine Detailangaben vorhanden sind
    ///  - niemals <c>null</c>
    /// </remarks>
    public List<BbpNeoIavBetroffenheitRaw> Betroffenheiten { get; set; } = [];
}
