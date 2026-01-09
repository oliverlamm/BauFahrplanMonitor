namespace BauFahrplanMonitor.Core.Importer.Dto.BbpNeo;

/// <summary>
/// Domain-DTO für IAV (Industrieanschluss-Verkehre)
/// im BBPNeo-Kontext.
///
/// Beschreibt, ob und wie Industrieanschlüsse
/// durch eine BVE / Regelung betroffen sind.
/// </summary>
/// <remarks>
/// IAV ist eine optionale Child-Struktur einer BVE
/// und ergänzt APS um betriebliche Auswirkungen
/// auf Industrieanschlüsse.
///
/// Architekturrolle:
/// <code>
/// BbpNeoMassnahme
///  └─ Regelung
///     └─ BVE
///        └─ IAV          ← HIER
/// </code>
///
/// Dieses DTO ist:
///  - vollständig normalisiert
///  - frei von XML-/RAW-Abhängigkeiten
///  - rein fachlich (kein DB-Entity)
///
/// Die fachliche Interpretation
/// (z. B. Betroffenheit ja/nein)
/// erfolgt im Normalizer.
/// </remarks>
public sealed class BbpNeoIav {

    /// <summary>
    /// Gibt an, ob Industrieanschlüsse betroffen sind.
    /// </summary>
    /// <remarks>
    /// <c>true</c>  → mindestens ein Industrieanschluss betroffen  
    /// <c>false</c> → keine Betroffenheit
    /// </remarks>
    public bool Betroffenheit { get; init; }

    /// <summary>
    /// Freitext-Beschreibung der IAV-Betroffenheit.
    /// </summary>
    /// <remarks>
    /// Kann erläuternde Hinweise enthalten,
    /// z. B. zur Art der Einschränkung oder
    /// betroffenen Anschlüsse.
    /// </remarks>
    public string? Beschreibung { get; init; }

    /// <summary>
    /// Liste der konkret betroffenen Industrieanschlüsse.
    /// </summary>
    /// <remarks>
    /// Enthält eine oder mehrere
    /// <see cref="BbpNeoIavBetroffenheit"/>-Einträge,
    /// die die Auswirkungen im Detail beschreiben.
    ///
    /// Die Liste ist:
    ///  - leer, wenn keine Detailangaben vorhanden sind
    ///  - niemals <c>null</c>
    /// </remarks>
    public List<BbpNeoIavBetroffenheit> Betroffenheiten { get; init; } = [];
    
    public bool IstBetroffen { get; set; } = false;
}