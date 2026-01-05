namespace BauFahrplanMonitor.Importer.Dto.BbpNeo;

/// <summary>
/// Domain-DTO für eine einzelne IAV-Betroffenheit
/// (Industrieanschluss-Verkehr) im BBPNeo-Kontext.
///
/// Beschreibt die konkreten Auswirkungen einer BVE
/// auf einen einzelnen Industrieanschlussvertrag.
/// </summary>
/// <remarks>
/// Dieses Objekt ist:
///  - Bestandteil von <see cref="BbpNeoIav"/>
///  - eindeutig identifizierbar über <see cref="VertragNr"/>
///  - vollständig normalisiert
///
/// Architekturrolle:
/// <code>
/// BbpNeoMassnahme
///  └─ Regelung
///     └─ BVE
///        └─ IAV
///           └─ IAV-Betroffenheit   ← HIER
/// </code>
///
/// Die enthaltenen Daten werden später:
///  - über Resolver (DS100 / VzG)
///  - im Upsert-Service
/// in relationale Strukturen überführt.
/// </remarks>
public sealed class BbpNeoIavBetroffenheit {

    // ==========================================================
    // IDENTITÄT
    // ==========================================================

    /// <summary>
    /// Vertragsnummer des Industrieanschlusses.
    /// </summary>
    /// <remarks>
    /// Fachlicher Primärschlüssel der IAV-Betroffenheit.
    ///
    /// Im Upsert gilt:
    ///  - dieses Feld MUSS gefüllt sein
    ///  - dient als Guard gegen Duplikate
    /// </remarks>
    public string VertragNr { get; set; } = "";

    // ==========================================================
    // REFERENZEN / LOKALISIERUNG
    // ==========================================================

    /// <summary>
    /// DS100-Code der zugehörigen Betriebsstelle.
    /// </summary>
    /// <remarks>
    /// Wird im Resolver auf eine
    /// <c>BasisBetriebsstelle</c> abgebildet.
    /// </remarks>
    public string? BstDs100 { get; set; }

    /// <summary>
    /// VzG-Nummer der zugehörigen Strecke.
    /// </summary>
    public long? VzgStrecke { get; set; }

    // ==========================================================
    // VERTRAGLICHE FACHFELDER
    // ==========================================================

    /// <summary>
    /// Kunde des Industrieanschlusses.
    /// </summary>
    public string? Kunde { get; set; }

    /// <summary>
    /// Anschlussgrenze des Industrieanschlusses.
    /// </summary>
    public string? Anschlussgrenze { get; set; }

    /// <summary>
    /// Art des Vertrags.
    /// </summary>
    public string? VertragArt { get; set; }

    /// <summary>
    /// Status des Vertrags.
    /// </summary>
    public string? VertragStatus { get; set; }

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

    // ==========================================================
    // BETRIEBLICHE AUSWIRKUNGEN
    // ==========================================================

    /// <summary>
    /// Einschränkung der Bedienbarkeit des Industrieanschlusses (IA).
    /// </summary>
    public string? EinschraenkungBedienbarkeitIA { get; set; }

    /// <summary>
    /// Freitext-Kommentar zur IAV-Betroffenheit.
    /// </summary>
    public string? Kommentar { get; set; }

    public bool IstBetroffen { get; set; } = false;
}
