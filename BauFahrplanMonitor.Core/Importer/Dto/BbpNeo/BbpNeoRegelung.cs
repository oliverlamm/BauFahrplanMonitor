namespace BauFahrplanMonitor.Core.Importer.Dto.BbpNeo;

/// <summary>
/// Domain-DTO für eine Regelung im BBPNeo-Kontext.
///
/// Eine Regelung beschreibt die betriebliche Umsetzung
/// einer Maßnahme in einem bestimmten Zeitraum und
/// räumlichen Abschnitt.
/// </summary>
/// <remarks>
/// Architekturrolle:
/// <code>
/// BbpNeoMassnahme
///  └─ Regelung                 ← HIER
///     └─ BVE (1..n)
///        ├─ APS
///        └─ IAV
/// </code>
///
/// Dieses DTO ist:
///  - vollständig normalisiert
///  - frei von XML-/RAW-Abhängigkeiten
///  - rein fachlich (kein EF-Entity)
///
/// Die Persistenz erfolgt im
/// <see cref="BbpNeoUpsertService"/>.
/// </remarks>
public sealed class BbpNeoRegelung {

    // ==========================================================
    // IDENTITÄT
    // ==========================================================

    /// <summary>
    /// Eindeutige Regelungs-ID (RegId).
    /// </summary>
    /// <remarks>
    /// Fachlicher Primärschlüssel der Regelung
    /// innerhalb einer Maßnahme.
    /// </remarks>
    public required string RegId { get; init; }

    /// <summary>
    /// Gibt an, ob die Regelung aktiv ist.
    /// </summary>
    public bool Aktiv { get; init; }

    // ==========================================================
    // ZEITLICHE GÜLTIGKEIT
    // ==========================================================

    /// <summary>
    /// Beginn der Regelung.
    /// </summary>
    public DateTime? Beginn { get; init; }

    /// <summary>
    /// Ende der Regelung.
    /// </summary>
    public DateTime? Ende { get; init; }

    /// <summary>
    /// Bauplanungsart (BPL-Art).
    /// </summary>
    /// <remarks>
    /// Fachliche Klassifikation der Regelung
    /// (z. B. Sperrung, Teilsperrung, Umleitung).
    /// </remarks>
    public string? BplArt { get; init; }

    /// <summary>
    /// Textuelle Beschreibung des Zeitraums.
    /// </summary>
    public string? Zeitraum { get; init; }

    /// <summary>
    /// Richtungsangabe der Regelung.
    /// </summary>
    /// <remarks>
    /// Typische Werte:
    ///  - 1 / -1
    ///  - auf / ab
    ///  - richtungsneutral (null)
    /// </remarks>
    public short? Richtung { get; init; }

    // ==========================================================
    // BESCHREIBUNG
    // ==========================================================

    /// <summary>
    /// Kurzbeschreibung der Regelung.
    /// </summary>
    public string? RegelungKurz { get; init; }

    /// <summary>
    /// Langbeschreibung der Regelung.
    /// </summary>
    public string? RegelungLang { get; init; }

    // ==========================================================
    // BETRIEBSFORM
    // ==========================================================

    /// <summary>
    /// Gibt an, ob die Regelung durchgehend gilt.
    /// </summary>
    public bool? Durchgehend { get; init; }

    /// <summary>
    /// Gibt an, ob die Regelung schichtweise gilt.
    /// </summary>
    public bool? Schichtweise { get; init; }

    // ==========================================================
    // RÄUMLICHE LAGE
    // ==========================================================

    /// <summary>
    /// DS100-Code der Abgangs-Betriebsstelle.
    /// </summary>
    public string? VonBstDs100 { get; init; }

    /// <summary>
    /// DS100-Code der Ziel-Betriebsstelle.
    /// </summary>
    public string? BisBstDs100 { get; init; }

    /// <summary>
    /// VzG-Nummer der Startstrecke.
    /// </summary>
    public long? VonVzG { get; init; }

    /// <summary>
    /// VzG-Nummer der Zielstrecke.
    /// </summary>
    public long? BisVzG { get; init; }

    // ==========================================================
    // CHILD-STRUKTUREN
    // ==========================================================

    /// <summary>
    /// Liste der zur Regelung gehörenden BVEs.
    /// </summary>
    /// <remarks>
    /// Jede BVE beschreibt eine konkrete
    /// betriebliche Verfahrensanordnung
    /// innerhalb dieser Regelung.
    ///
    /// Die Liste ist:
    ///  - niemals <c>null</c>
    ///  - kann leer sein
    /// </remarks>
    public IReadOnlyList<BbpNeoBve> Bven { get; init; } = [];
}
