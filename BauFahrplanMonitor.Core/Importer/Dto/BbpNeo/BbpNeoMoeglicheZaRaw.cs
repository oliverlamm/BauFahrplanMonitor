namespace BauFahrplanMonitor.Importer.Dto.BbpNeo;

/// <summary>
/// RAW-DTO für eine mögliche Zugart (ZA)
/// aus dem BBPNeo-XML.
///
/// Wird im Kontext von APS-Betroffenheiten verwendet,
/// um zusätzliche fachliche Einschränkungen oder
/// Gültigkeiten abzubilden.
/// </summary>
/// <remarks>
/// Dieses DTO ist:
///  - XML-nah
///  - vollständig unverarbeitet
///  - bewusst ohne Typkonvertierung oder Validierung
///
/// Zweck:
/// <list type="bullet">
///   <item>verlustfreie Übernahme möglicher Zugarten</item>
///   <item>optionale fachliche Auswertung im Normalizer oder Upsert</item>
/// </list>
///
/// Architekturrolle:
/// <code>
/// XML
///  ↓
/// BbpNeoMoeglicheZaRaw      ← HIER (RAW)
///  ↓
/// BbpNeoNormalizer
///  ↓
/// (optional) fachliche Interpretation
/// </code>
///
/// In dieser Klasse darf **keine fachliche Logik**
/// implementiert werden.
/// </remarks>
public class BbpNeoMoeglicheZaRaw {

    /// <summary>
    /// UUID der möglichen Zugart.
    /// </summary>
    /// <remarks>
    /// Dient der eindeutigen Identifikation
    /// innerhalb der XML-Struktur.
    /// </remarks>
    public string? UuidZa { get; set; }

    /// <summary>
    /// Typ der Zugart (RAW).
    /// </summary>
    /// <remarks>
    /// Fachliche Klassifikation der Zugart,
    /// z. B. bestimmte Betriebs- oder Fahrzeugtypen.
    /// </remarks>
    public string? TypZa { get; set; }

    /// <summary>
    /// Objektnummer der Zugart (RAW).
    /// </summary>
    /// <remarks>
    /// Referenziert ein externes Objekt
    /// oder eine systeminterne Kennung.
    /// </remarks>
    public string? Objektnummer { get; set; }
}