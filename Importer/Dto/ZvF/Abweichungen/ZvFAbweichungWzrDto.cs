namespace BauFahrplanMonitor.Importer.Dto.ZvF.Abweichungen;

/// <summary>
/// Beschreibt eine WZR-Abweichung (Weisung / Zusatzregel)
/// im Rahmen einer ZvF-Abweichung.
///
/// WZR-Abweichungen sind in der Regel:
/// <list type="bullet">
///   <item>textuell geprägt</item>
///   <item>betriebsorganisatorisch</item>
///   <item>nicht zwingend zeit- oder streckenbezogen</item>
/// </list>
///
/// Dieses DTO modelliert:
/// <list type="bullet">
///   <item>die Art der WZR</item>
///   <item>den räumlichen Geltungsbereich</item>
///   <item>den erläuternden Text</item>
/// </list>
///
/// Es handelt sich um:
/// <list type="bullet">
///   <item>ein Regelungs-DTO</item>
///   <item>ohne eigenen Zug- oder Zeitbezug</item>
///   <item>einen Bestandteil einer ZvF-Abweichung</item>
/// </list>
/// </summary>
/// <remarks>
/// Architekturrolle:
/// <code>
/// ZvF-Abweichung (XML)
///   ↓
/// Normalizer
///   ↓
/// ZvFAbweichungWzrDto        ← HIER
///   ↓
/// ZvFAbweichungDto (JsonRaw)
///   ↓
/// Upsert / Persistenz
/// </code>
///
/// Die Zuordnung zu:
/// <list type="bullet">
///   <item>Zugnummer</item>
///   <item>Verkehrstag</item>
///   <item>Abweichungsart</item>
/// </list>
/// erfolgt immer über das übergeordnete <see cref="ZvFAbweichungDto"/>.
/// </remarks>
public class ZvFAbweichungWzrDto {

    // ==========================================================
    // WZR-ART
    // ==========================================================

    /// <summary>
    /// Art der WZR (Weisung / Zusatzregel).
    ///
    /// Typische Beispiele:
    /// <list type="bullet">
    ///   <item><c>WZ</c></item>
    ///   <item><c>WR</c></item>
    ///   <item><c>HZ</c></item>
    /// </list>
    ///
    /// Die konkrete Bedeutung wird
    /// fachlich über <see cref="Text"/> transportiert.
    /// </summary>
    public string? Art { get; set; }

    // ==========================================================
    // GELTUNGSBEREICH
    // ==========================================================

    /// <summary>
    /// RL100-/DS100-Code der Betriebsstelle,
    /// in deren Bereich die WZR gilt.
    ///
    /// Kann <c>null</c> sein, wenn:
    /// <list type="bullet">
    ///   <item>die Regel global gilt</item>
    ///   <item>kein expliziter Ort angegeben ist</item>
    /// </list>
    /// </summary>
    public string? GiltInRl100 { get; set; }

    // ==========================================================
    // TEXT
    // ==========================================================

    /// <summary>
    /// Vollständiger Beschreibungstext der WZR.
    ///
    /// Der Text ist:
    /// <list type="bullet">
    ///   <item>fachlich maßgeblich</item>
    ///   <item>nicht weiter strukturiert</item>
    ///   <item>direkt aus dem ZvF-XML übernommen</item>
    /// </list>
    /// </summary>
    public string? Text { get; set; }
}
