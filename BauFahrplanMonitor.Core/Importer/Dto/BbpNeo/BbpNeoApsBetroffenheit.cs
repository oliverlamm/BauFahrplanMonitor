namespace BauFahrplanMonitor.Core.Importer.Dto.BbpNeo;

/// <summary>
/// Domain-DTO für eine einzelne APS-Betroffenheit.
///
/// Beschreibt eine konkret betroffene Anlage, ein Gleis
/// oder einen technischen Platz im Rahmen einer APS-Regelung.
/// </summary>
/// <remarks>
/// Dieses Objekt ist:
///  - Bestandteil von <see cref="BbpNeoAps"/>
///  - eindeutig identifizierbar über <see cref="Uuid"/>
///  - vollständig normalisiert (keine RAW-/XML-Abhängigkeiten)
///
/// Architekturrolle:
/// <code>
/// BbpNeoMassnahme
///  └─ Regelung
///     └─ BVE
///        └─ APS
///           └─ APS-Betroffenheit   ← HIER
/// </code>
///
/// Die enthaltenen Daten werden später:
///  - über Resolver (z. B. DS100 → Betriebsstelle)
///  - im Upsert-Service
/// in relationale Strukturen überführt.
/// </remarks>
public sealed class BbpNeoApsBetroffenheit {

    // ==========================================================
    // IDENTITÄT
    // ==========================================================

    /// <summary>
    /// Eindeutige UUID der APS-Betroffenheit.
    /// </summary>
    /// <remarks>
    /// Wird im Upsert als fachlicher Schlüssel verwendet
    /// (unique pro BVE).
    ///
    /// Dieses Feld ist:
    ///  - zwingend erforderlich
    ///  - stabil über Reimporte hinweg
    /// </remarks>
    public string Uuid { get; set; } = "";

    // ==========================================================
    // FACHLICHE METADATEN
    // ==========================================================

    /// <summary>
    /// Ab welchem Fahrplanjahr die Betroffenheit gilt.
    /// </summary>
    /// <remarks>
    /// Optionales Feld; kann <c>null</c> sein,
    /// wenn keine zeitliche Einschränkung angegeben ist.
    /// </remarks>
    public int? AbFahrplanjahr { get; set; }

    /// <summary>
    /// DS100-Code der betroffenen Betriebsstelle.
    /// </summary>
    /// <remarks>
    /// Wird über den
    /// <c>SharedReferenceResolver</c>
    /// in eine Datenbank-Referenz aufgelöst.
    /// </remarks>
    public string? BstDs100 { get; set; }

    /// <summary>
    /// Gleisnummer oder Gleisbezeichnung.
    /// </summary>
    public string? Gleis { get; set; }

    /// <summary>
    /// Primäre Kategorie der Betroffenheit.
    /// </summary>
    /// <remarks>
    /// Fachliche Klassifikation
    /// (z. B. Sicherungstechnik, Energie, Leit- und Sicherung).
    /// </remarks>
    public string? PrimaereKat { get; set; }

    /// <summary>
    /// Sekundäre Kategorie der Betroffenheit.
    /// </summary>
    public string? SekundaerKat { get; set; }

    // ==========================================================
    // TECHNISCHE MERKMALE
    // ==========================================================

    /// <summary>
    /// Gibt an, ob die Oberleitung betroffen ist.
    /// </summary>
    public bool? Oberleitung { get; set; }

    /// <summary>
    /// Gibt an, ob die Oberleitung abgeschaltet ist.
    /// </summary>
    public bool? OberleitungAus { get; set; }

    /// <summary>
    /// Technischer Platz (z. B. Schaltanlage, Mast, Anlage).
    /// </summary>
    public string? TechnischerPlatz { get; set; }

    /// <summary>
    /// Art der technischen Anbindung.
    /// </summary>
    public string? ArtDerAnbindung { get; set; }

    // ==========================================================
    // BETRIEBLICHE AUSWIRKUNGEN
    // ==========================================================

    /// <summary>
    /// Einschränkung der Befahrbarkeit (SE = StreckenEinschränkung).
    /// </summary>
    /// <remarks>
    /// Beschreibt, in welcher Form die Befahrbarkeit
    /// durch die Maßnahme eingeschränkt ist.
    /// </remarks>
    public string? EinschraenkungBefahrbarkeitSe { get; set; }

    /// <summary>
    /// Freitext-Kommentar zur Betroffenheit.
    /// </summary>
    public string? Kommentar { get; set; }

    /// <summary>
    /// Liste möglicher Zugarten / ZAs, die betroffen sind.
    /// </summary>
    /// <remarks>
    /// Wird aus dem RAW-XML übernommen und
    /// ggf. später fachlich ausgewertet.
    ///
    /// Die Liste ist niemals <c>null</c>.
    /// </remarks>
    public List<BbpNeoZaRaw> MoeglicheZas { get; set; } = [];

    public bool IstBetroffen { get; set; } = false;
}