using System.Collections.Generic;

namespace BauFahrplanMonitor.Importer.Dto.BbpNeo;

/// <summary>
/// RAW-DTO für eine BBPNeo-Maßnahme aus dem XML.
///
/// Enthält alle maßnahmenbezogenen Informationen
/// unverarbeitet und XML-nah als String-Werte.
/// </summary>
/// <remarks>
/// Dieses DTO ist:
///  - Ergebnis des XML-Streamings
///  - vollständig XML-nah
///  - bewusst ohne Typkonvertierung oder Validierung
///
/// Zweck:
/// <list type="bullet">
///   <item>verlustfreie Übernahme der kompletten Maßnahme</item>
///   <item>Eingabe für den <see cref="BbpNeoNormalizer"/></item>
/// </list>
///
/// Architekturrolle:
/// <code>
/// XML
///  ↓
/// BbpNeoMassnahmeRaw        ← HIER (RAW)
///  ↓
/// BbpNeoNormalizer
///  ↓
/// BbpNeoMassnahme           (Domain / Aggregatwurzel)
/// </code>
///
/// In dieser Klasse darf **keine fachliche Logik**
/// implementiert werden.
/// </remarks>
public sealed class BbpNeoMassnahmeRaw {

    // ==========================================================
    // IDENTITÄT / STATUS (RAW)
    // ==========================================================

    /// <summary>
    /// Eindeutige Maßnahmen-ID aus dem XML (RAW).
    /// </summary>
    /// <remarks>
    /// Wird im Normalizer als Pflichtfeld geprüft
    /// und als fachlicher Schlüssel übernommen.
    /// </remarks>
    public string? MasId { get; set; }

    /// <summary>
    /// Kennzeichnung, ob die Maßnahme aktiv ist (RAW).
    /// </summary>
    /// <remarks>
    /// Typische Werte:
    ///  - "0" / "1"
    ///  - "true" / "false"
    /// </remarks>
    public string? Aktiv { get; set; }

    // ==========================================================
    // METADATEN
    // ==========================================================

    /// <summary>
    /// Vorhaben / Projektbezeichnung der Maßnahme (RAW).
    /// </summary>
    public string? Vorhaben { get; set; }

    /// <summary>
    /// Region-ID der Maßnahme (RAW).
    /// </summary>
    public string? RegionId { get; set; }

    /// <summary>
    /// Regionsbezeichnung der Maßnahme (RAW).
    /// </summary>
    public string? Region { get; set; }

    // ==========================================================
    // ARBEITEN (RAW)
    // ==========================================================

    /// <summary>
    /// Beschreibung der auszuführenden Arbeiten (RAW).
    /// </summary>
    public string? Arbeiten { get; set; }

    /// <summary>
    /// Art der Arbeiten (RAW).
    /// </summary>
    public string? ArtDerArbeiten { get; set; }

    // ==========================================================
    // STRECKE / BETRIEBSSTELLEN (RAW)
    // ==========================================================

    /// <summary>
    /// VzG-Streckennummer Beginn (RAW).
    /// </summary>
    public string? VzGStrecke { get; set; }

    /// <summary>
    /// VzG-Streckennummer Ende (RAW).
    /// </summary>
    public string? VzGStreckeBis { get; set; }

    /// <summary>
    /// DS100-/Ril100-Code der Abgangs-Betriebsstelle (RAW).
    /// </summary>
    public string? MasBstVonRil100 { get; set; }

    /// <summary>
    /// DS100-/Ril100-Code der Ziel-Betriebsstelle (RAW).
    /// </summary>
    public string? MasBstBisRil100 { get; set; }

    /// <summary>
    /// Kilometerlage Beginn der Maßnahme (RAW).
    /// </summary>
    public string? MasKmVon { get; set; }

    /// <summary>
    /// Kilometerlage Ende der Maßnahme (RAW).
    /// </summary>
    public string? MasKmBis { get; set; }

    // ==========================================================
    // ZEITEN (RAW)
    // ==========================================================

    /// <summary>
    /// Beginn der Maßnahme (RAW).
    /// </summary>
    /// <remarks>
    /// Wird im Normalizer zu <see cref="System.DateTime"/>
    /// oder <c>null</c> konvertiert.
    /// </remarks>
    public string? MasBeginn { get; set; }

    /// <summary>
    /// Ende der Maßnahme (RAW).
    /// </summary>
    public string? MasEnde { get; set; }

    // ==========================================================
    // GENEHMIGUNG / FORMALIA (RAW)
    // ==========================================================

    /// <summary>
    /// Anmeldedatum der Maßnahme (RAW).
    /// </summary>
    public string? Anmeldung { get; set; }

    /// <summary>
    /// Genehmigungsstatus oder -art (RAW).
    /// </summary>
    public string? Genehmigung { get; set; }

    /// <summary>
    /// Datum des Auftrags an die BBZR (RAW).
    /// </summary>
    public string? DatumAuftragBBZR { get; set; }

    // ==========================================================
    // CHILD-STRUKTUREN (RAW)
    // ==========================================================

    /// <summary>
    /// Liste der zugehörigen Regelungen (RAW).
    /// </summary>
    /// <remarks>
    /// Enthält eine oder mehrere
    /// <see cref="BbpNeoRegelungRaw"/>-Einträge.
    ///
    /// Die Liste ist:
    ///  - niemals <c>null</c>
    ///  - kann leer sein
    /// </remarks>
    public List<BbpNeoRegelungRaw> Regelungen { get; set; } = [];
}
