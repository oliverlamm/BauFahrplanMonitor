namespace BauFahrplanMonitor.Core.Importer.Dto.ZvF.Abweichungen;

/// <summary>
/// Interner Container für mehrere WZR-Regeln
/// im Rahmen einer ZvF-Abweichung.
///
/// Diese Klasse dient ausschließlich dazu,
/// eine Liste von <see cref="ZvFAbweichungWzrDto"/>
/// strukturiert zu kapseln (z. B. für JSON-Serialisierung).
///
/// Es handelt sich um:
/// <list type="bullet">
///   <item>einen technischen Hilfstyp</item>
///   <item>keine eigenständige fachliche Regel</item>
///   <item>eine reine Strukturklasse</item>
/// </list>
/// </summary>
/// <remarks>
/// Sichtbarkeit:
/// <list type="bullet">
///   <item><c>internal</c> – nur innerhalb des ZvF-Importers</item>
/// </list>
///
/// Architekturrolle:
/// <code>
/// ZvF-Abweichung (XML)
///   ↓
/// Normalizer
///   ↓
/// ZvFAbweichungWzrListeDto     ← HIER
///   ↓
/// JSON-Serialisierung
///   ↓
/// ZvFAbweichungDto.JsonRaw
/// </code>
///
/// Diese Klasse existiert primär:
/// <list type="bullet">
///   <item>zur sauberen Gruppierung mehrerer WZR-Einträge</item>
///   <item>zur Stabilisierung der JSON-Struktur</item>
/// </list>
/// </remarks>
internal class ZvFAbweichungWzrListeDto {
    // ==========================================================
    // REGELLISTE
    // ==========================================================

    /// <summary>
    /// Liste der enthaltenen WZR-Regeln.
    ///
    /// Jeder Eintrag beschreibt:
    /// <list type="bullet">
    ///   <item>eine einzelne Weisung / Zusatzregel</item>
    ///   <item>mit optionalem räumlichen Bezug</item>
    ///   <item>und erläuterndem Text</item>
    /// </list>
    /// </summary>
    public List<ZvFAbweichungWzrDto> RegelungListe { get; set; } = [];
}