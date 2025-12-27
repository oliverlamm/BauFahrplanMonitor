using System.Collections.Generic;

namespace BauFahrplanMonitor.Importer.Dto.BbpNeo;

/// <summary>
/// RAW-DTO für eine Regelung aus dem BBPNeo-XML.
///
/// Enthält alle regelungsbezogenen Informationen
/// unverarbeitet und XML-nah als String-Werte.
/// </summary>
/// <remarks>
/// Dieses DTO ist:
///  - Ergebnis des Streaming-Parsers
///  - vollständig XML-nah
///  - bewusst ohne Typkonvertierung oder Validierung
///
/// Zweck:
/// <list type="bullet">
///   <item>verlustfreie Übernahme aller Regelungsdaten</item>
///   <item>Eingabe für den <see cref="BbpNeoNormalizer"/></item>
/// </list>
///
/// Architekturrolle:
/// <code>
/// XML
///  ↓
/// BbpNeoRegelungRaw        ← HIER (RAW)
///  ↓
/// BbpNeoNormalizer
///  ↓
/// BbpNeoRegelung           (Domain)
/// </code>
///
/// In dieser Klasse darf **keine fachliche Logik**
/// implementiert werden.
/// </remarks>
public sealed class BbpNeoRegelungRaw {
    // ==========================================================
    // IDENTITÄT (RAW)
    // ==========================================================

    /// <summary>
    /// Eindeutige Regelungs-ID aus dem XML (RAW).
    /// </summary>
    /// <remarks>
    /// Wird im Normalizer als Pflichtfeld geprüft
    /// und als fachlicher Schlüssel übernommen.
    /// </remarks>
    public string? RegId { get; set; }

    // ==========================================================
    // STATUS (RAW)
    // ==========================================================

    /// <summary>
    /// Kennzeichnung, ob die Regelung aktiv ist (RAW).
    /// </summary>
    /// <remarks>
    /// Typische Werte:
    ///  - "0" / "1"
    ///  - "true" / "false"
    /// </remarks>
    public string? Aktiv { get; set; }

    // ==========================================================
    // ZEITLICHE ANGABEN (RAW)
    // ==========================================================

    /// <summary>
    /// Beginn der Regelung (RAW).
    /// </summary>
    /// <remarks>
    /// Wird im Normalizer zu <see cref="System.DateTime"/>
    /// oder <c>null</c> konvertiert.
    /// </remarks>
    public string? Beginn { get; set; }

    /// <summary>
    /// Ende der Regelung (RAW).
    /// </summary>
    public string? Ende { get; set; }

    /// <summary>
    /// Textuelle Beschreibung des Zeitraums (RAW).
    /// </summary>
    public string? Zeitraum { get; set; }

    // ==========================================================
    // STRECKE / RICHTUNG (RAW)
    // ==========================================================

    /// <summary>
    /// DS100-/Ril100-Code der Abgangs-Betriebsstelle (RAW).
    /// </summary>
    public string? BstVonRil100 { get; set; }

    /// <summary>
    /// DS100-/Ril100-Code der Ziel-Betriebsstelle (RAW).
    /// </summary>
    public string? BstBisRil100 { get; set; }

    /// <summary>
    /// VzG-Streckennummer Beginn (RAW).
    /// </summary>
    public string? VzGStrecke { get; set; }

    /// <summary>
    /// VzG-Streckennummer Ende (RAW).
    /// </summary>
    public string? VzGStreckeBis { get; set; }

    /// <summary>
    /// Richtungsangabe der Regelung (RAW).
    /// </summary>
    /// <remarks>
    /// Typische Werte:
    ///  - "auf" / "ab"
    ///  - "1" / "-1"
    /// </remarks>
    public string? Richtung { get; set; }

    // ==========================================================
    // REGELUNGSART / BESCHREIBUNG (RAW)
    // ==========================================================

    /// <summary>
    /// Bauplanungsart der Regelung (RAW).
    /// </summary>
    public string? BplArtText { get; set; }

    /// <summary>
    /// Kurzbeschreibung der Regelung (RAW).
    /// </summary>
    public string? BplRegelungKurz { get; set; }

    /// <summary>
    /// Langbeschreibung der Regelung (RAW).
    /// </summary>
    public string? BplRegelungLang { get; set; }

    // ==========================================================
    // FLAGS (RAW)
    // ==========================================================

    /// <summary>
    /// Kennzeichnung, ob die Regelung durchgehend gilt (RAW).
    /// </summary>
    public string? Durchgehend { get; set; }

    /// <summary>
    /// Kennzeichnung, ob die Regelung schichtweise gilt (RAW).
    /// </summary>
    public string? Schichtweise { get; set; }

    // ==========================================================
    // BEMERKUNGEN (RAW)
    // ==========================================================

    /// <summary>
    /// Bemerkungen aus der Bauplanung (RAW).
    /// </summary>
    public string? BemerkungenBpl { get; set; }

    /// <summary>
    /// Bemerkungen aus der Fahrplanung (RAW).
    /// </summary>
    public string? BemerkungenFpl { get; set; }

    // ==========================================================
    // CHILD-STRUKTUREN (RAW)
    // ==========================================================

    /// <summary>
    /// Liste der zugehörigen BVEs (RAW).
    /// </summary>
    /// <remarks>
    /// Enthält eine oder mehrere
    /// <see cref="BbpNeoBveRaw"/>-Einträge.
    ///
    /// Die Liste ist:
    ///  - niemals <c>null</c>
    ///  - kann leer sein
    /// </remarks>
    public List<BbpNeoBveRaw> Bven { get; set; } = [];
}