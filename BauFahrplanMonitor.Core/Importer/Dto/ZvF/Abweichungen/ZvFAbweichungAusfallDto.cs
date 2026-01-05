namespace BauFahrplanMonitor.Importer.Dto.ZvF.Abweichungen;

/// <summary>
/// Beschreibt den räumlichen Ausfallbereich einer Zugabweichung
/// im ZvF-Export-Kontext.
///
/// Dieses DTO modelliert ausschließlich:
/// <list type="bullet">
///   <item>den Beginn des Ausfalls</item>
///   <item>das Ende des Ausfalls</item>
/// </list>
///
/// Beide Angaben erfolgen als RL100-/DS100-Codes
/// und werden im Upsert-Layer auf Betriebsstellen aufgelöst.
///
/// Es handelt sich um:
/// <list type="bullet">
///   <item>ein fachliches Regelungs-DTO</item>
///   <item>ohne eigenen Zug- oder Zeitbezug</item>
///   <item>einen Teilaspekt einer ZvF-Abweichung</item>
/// </list>
/// </summary>
/// <remarks>
/// Architekturrolle:
/// <code>
/// ZvFAbweichungRaw
///   ↓
/// Normalizer / Resolver
///   ↓
/// ZvFAbweichungAusfallDto        ← HIER
///   ↓
/// ZvFAbweichungDto
///   ↓
/// BusinessLogic / Upsert
/// </code>
///
/// Der eigentliche Kontext (Zugnummer, Verkehrstag, Abweichungsart)
/// liegt immer im übergeordneten Abweichungs-DTO.
/// </remarks>
public class ZvFAbweichungAusfallDto {

    // ==========================================================
    // AUSFALLSTRECKE
    // ==========================================================

    /// <summary>
    /// RL100-/DS100-Code der Betriebsstelle,
    /// ab der der Zug ausfällt.
    /// </summary>
    public string? AusfallAbRl100 { get; set; }

    /// <summary>
    /// RL100-/DS100-Code der Betriebsstelle,
    /// bis zu der der Zug ausfällt.
    /// </summary>
    public string? AusfallBisRl100 { get; set; }
}