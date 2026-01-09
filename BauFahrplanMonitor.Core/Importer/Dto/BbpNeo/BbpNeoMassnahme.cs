namespace BauFahrplanMonitor.Core.Importer.Dto.BbpNeo;

/// <summary>
/// Root-Domain-DTO für eine BBPNeo-Maßnahme.
///
/// Repräsentiert eine einzelne Baumaßnahme
/// inklusive zeitlicher, räumlicher und fachlicher Metadaten
/// sowie aller zugehörigen Regelungen.
/// </summary>
/// <remarks>
/// Architekturrolle:
/// <code>
/// BbpNeoMassnahme        ← ROOT
///  └─ Regelung (1..n)
///     └─ BVE
///        ├─ APS
///        └─ IAV
/// </code>
///
/// Dieses Objekt ist:
///  - vollständig normalisiert
///  - frei von XML-/RAW-Abhängigkeiten
///  - fachlich stabil (kein EF-Entity)
///
/// Es stellt die zentrale Aggregatwurzel dar,
/// die im <see cref="BbpNeoUpsertService"/>
/// transaktional persistiert wird.
/// </remarks>
public sealed class BbpNeoMassnahme {

    // ==========================================================
    // IDENTITÄT
    // ==========================================================

    /// <summary>
    /// Eindeutige Maßnahmen-ID (MasId).
    /// </summary>
    /// <remarks>
    /// Fachlicher Primärschlüssel der Maßnahme.
    ///
    /// Dieses Feld ist zwingend erforderlich
    /// und dient als Identität über Reimporte hinweg.
    /// </remarks>
    public required string MasId { get; init; }

    /// <summary>
    /// Gibt an, ob die Maßnahme aktiv ist.
    /// </summary>
    public bool Aktiv { get; init; }

    /// <summary>
    /// Referenz auf die zuständige Region.
    /// </summary>
    /// <remarks>
    /// Wird im Resolver auf eine
    /// <c>Region</c>-Entität abgebildet.
    /// </remarks>
    public long? RegionId { get; init; }

    // ==========================================================
    // FACHLICHE BASISDATEN
    // ==========================================================

    /// <summary>
    /// Beschreibung der auszuführenden Arbeiten.
    /// </summary>
    public string? Arbeiten { get; init; }

    /// <summary>
    /// Art der Arbeit (fachliche Klassifikation).
    /// </summary>
    public string? ArtDerArbeit { get; init; }

    /// <summary>
    /// Genehmigungsstatus oder Genehmigungsart.
    /// </summary>
    public string? Genehmigung { get; init; }

    // ==========================================================
    // ZEITLICHE EINORDNUNG
    // ==========================================================

    /// <summary>
    /// Beginn der Maßnahme.
    /// </summary>
    public DateTime? Beginn { get; init; }

    /// <summary>
    /// Ende der Maßnahme.
    /// </summary>
    public DateTime? Ende { get; init; }

    /// <summary>
    /// Datum der Anforderung bei der BBZR.
    /// </summary>
    public DateOnly? AnforderungBbzr { get; init; }

    /// <summary>
    /// Datum der Anmeldung der Maßnahme.
    /// </summary>
    public DateOnly? Anmeldung { get; set; }

    // ==========================================================
    // RÄUMLICHE LAGE (STRECKE / BST)
    // ==========================================================

    /// <summary>
    /// DS100-Code der Abgangs-Betriebsstelle der Maßnahme.
    /// </summary>
    public string? MasVonBstDs100 { get; set; }

    /// <summary>
    /// DS100-Code der Ziel-Betriebsstelle der Maßnahme.
    /// </summary>
    public string? MasBisBstDs100 { get; set; }

    /// <summary>
    /// VzG-Nummer der Startstrecke.
    /// </summary>
    public long? MasVonVzG { get; set; }

    /// <summary>
    /// VzG-Nummer der Zielstrecke.
    /// </summary>
    public long? MasBisVzG { get; set; }

    /// <summary>
    /// Kilometerlage Beginn (RAW-nahe Angabe).
    /// </summary>
    public string? MasVonKmL { get; init; }

    /// <summary>
    /// Kilometerlage Ende (RAW-nahe Angabe).
    /// </summary>
    public string? MasBisKmL { get; init; }

    // ==========================================================
    // CHILD-STRUKTUREN
    // ==========================================================

    /// <summary>
    /// Liste der zugehörigen Regelungen.
    /// </summary>
    /// <remarks>
    /// Jede Regelung beschreibt eine konkrete
    /// betriebliche Umsetzung der Maßnahme.
    ///
    /// Die Liste ist:
    ///  - niemals <c>null</c>
    ///  - kann leer sein
    /// </remarks>
    public IReadOnlyList<BbpNeoRegelung> Regelungen { get; init; } = [];
}
