using System.Collections.Generic;

namespace BauFahrplanMonitor.Importer.Dto.BbpNeo;

/// <summary>
/// RAW-DTO für eine APS-Betroffenheit aus dem BBPNeo-XML.
///
/// Enthält die unverarbeiteten, textuellen Werte
/// direkt aus dem XML-Dokument.
/// </summary>
/// <remarks>
/// Dieses DTO ist:
///  - Ergebnis des <see cref="BbpNeoRawXmlParser"/>
///  - vollständig XML-nah
///  - bewusst ohne Typkonvertierungen
///
/// Es dient ausschließlich als:
/// <list type="bullet">
///   <item>verlustfreie Zwischenrepräsentation</item>
///   <item>Eingabe für den <see cref="BbpNeoNormalizer"/></item>
/// </list>
///
/// Architekturrolle:
/// <code>
/// XML
///  ↓
/// BbpNeoApsBetroffenheitRaw   ← HIER (RAW)
///  ↓
/// BbpNeoNormalizer
///  ↓
/// BbpNeoApsBetroffenheit     (Domain)
/// </code>
///
/// Änderungen an diesem DTO dürfen **niemals**
/// fachliche Logik enthalten.
/// </remarks>
public sealed class BbpNeoApsBetroffenheitRaw {

    // ==========================================================
    // IDENTITÄT (RAW)
    // ==========================================================

    /// <summary>
    /// UUID der APS-Betroffenheit aus dem XML.
    /// </summary>
    /// <remarks>
    /// Wird im Normalizer geprüft und
    /// als fachlicher Schlüssel übernommen.
    /// </remarks>
    public string? Uuid { get; set; }

    /// <summary>
    /// Fahrplanjahr ab dem die Betroffenheit gilt (RAW).
    /// </summary>
    /// <remarks>
    /// Textueller Wert aus dem XML,
    /// wird im Normalizer zu <c>int?</c> konvertiert.
    /// </remarks>
    public string? AbFahrplanjahr { get; set; }

    // ==========================================================
    // LOKALISIERUNG
    // ==========================================================

    /// <summary>
    /// DS100-Code der betroffenen Betriebsstelle (RAW).
    /// </summary>
    /// <remarks>
    /// Wird im Normalizer ggf. bereinigt
    /// und später über Resolver aufgelöst.
    /// </remarks>
    public string? Ds100 { get; set; }

    /// <summary>
    /// Gleisnummer oder Gleisbezeichnung (RAW).
    /// </summary>
    public string? GleisNr { get; set; }

    // ==========================================================
    // KATEGORISIERUNG
    // ==========================================================

    /// <summary>
    /// Primäre Kategorie der Betroffenheit (RAW).
    /// </summary>
    public string? PrimaereKategorie { get; set; }

    /// <summary>
    /// Sekundäre Kategorie der Betroffenheit (RAW).
    /// </summary>
    public string? SekundaereKategorie { get; set; }

    // ==========================================================
    // TECHNISCHE MERKMALE (RAW)
    // ==========================================================

    /// <summary>
    /// Kennzeichnung, ob die Oberleitung betroffen ist (RAW).
    /// </summary>
    /// <remarks>
    /// Typischerweise "0", "1", "true", "false", "ja", …
    /// → Konvertierung erfolgt im Normalizer.
    /// </remarks>
    public string? Oberleitung { get; set; }

    /// <summary>
    /// Kennzeichnung, ob die Oberleitung abgeschaltet ist (RAW).
    /// </summary>
    public string? OberleitungAus { get; set; }

    /// <summary>
    /// Technischer Platz (RAW).
    /// </summary>
    public string? TechnischerPlatz { get; set; }

    /// <summary>
    /// Art der technischen Anbindung (RAW).
    /// </summary>
    public string? ArtDerAnbindung { get; set; }

    // ==========================================================
    // BETRIEBLICHE AUSWIRKUNGEN (RAW)
    // ==========================================================

    /// <summary>
    /// Einschränkung der Befahrbarkeit (SE) als RAW-Text.
    /// </summary>
    public string? EinschraenkungBefahrbarkeitSE { get; set; }

    /// <summary>
    /// Freitext-Kommentar zur Betroffenheit (RAW).
    /// </summary>
    public string? Kommentar { get; set; }

    // ==========================================================
    // ZUSATZSTRUKTUREN
    // ==========================================================

    /// <summary>
    /// Liste möglicher Zusatzausrüstungen (ZA) aus dem XML.
    /// </summary>
    /// <remarks>
    /// Wird unverändert übernommen
    /// und ggf. später fachlich ausgewertet.
    ///
    /// Die Liste ist niemals <c>null</c>.
    /// </remarks>
    public List<BbpNeoZaRaw>? MoeglicheZa { get; set; } = [];
    
}
