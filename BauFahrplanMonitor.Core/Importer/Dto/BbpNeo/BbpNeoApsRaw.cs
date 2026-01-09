namespace BauFahrplanMonitor.Core.Importer.Dto.BbpNeo;

/// <summary>
/// RAW-DTO für APS (Anlagenbezogene Sperrungen) aus dem BBPNeo-XML.
///
/// Enthält die unverarbeiteten, textuellen Werte
/// direkt aus der XML-Struktur.
/// </summary>
/// <remarks>
/// Dieses DTO ist:
///  - Ergebnis des XML-Streamings / RAW-Parsings
///  - vollständig XML-nah
///  - bewusst ohne Typkonvertierung oder Validierung
///
/// Zweck:
/// <list type="bullet">
///   <item>verlustfreie Übernahme aller APS-Informationen</item>
///   <item>Eingabe für den <see cref="BbpNeoNormalizer"/></item>
/// </list>
///
/// Architekturrolle:
/// <code>
/// XML
///  ↓
/// BbpNeoApsRaw            ← HIER (RAW)
///  ↓
/// BbpNeoNormalizer
///  ↓
/// BbpNeoAps               (Domain)
/// </code>
///
/// In dieser Klasse darf **keine fachliche Logik**
/// implementiert werden.
/// </remarks>
public sealed class BbpNeoApsRaw {

    // ==========================================================
    // KOPF-INFORMATIONEN (RAW)
    // ==========================================================

    /// <summary>
    /// Kennzeichnung, ob grundsätzlich eine Anlagenbetroffenheit vorliegt (RAW).
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
    /// Freitext-Beschreibung der Anlagenbetroffenheit (RAW).
    /// </summary>
    public string? Beschreibung { get; set; }

    /// <summary>
    /// Kennzeichnung, ob der Bereich frei von Fahrzeugen sein muss (RAW).
    /// </summary>
    /// <remarks>
    /// Wird im Normalizer zu <c>bool?</c> konvertiert.
    /// </remarks>
    public string? FreiVonFahrzeugen { get; set; }

    // ==========================================================
    // DETAILSTRUKTUREN
    // ==========================================================

    /// <summary>
    /// Liste der einzelnen betroffenen Anlagen / Gleise (RAW).
    /// </summary>
    /// <remarks>
    /// Enthält eine oder mehrere
    /// <see cref="BbpNeoApsBetroffenheitRaw"/>-Strukturen,
    /// die die Betroffenheit im Detail beschreiben.
    ///
    /// Die Liste ist:
    ///  - leer, wenn keine Detailangaben vorhanden sind
    ///  - niemals <c>null</c>
    /// </remarks>
    public List<BbpNeoApsBetroffenheitRaw> Betroffenheiten { get; set; } = [];
}
