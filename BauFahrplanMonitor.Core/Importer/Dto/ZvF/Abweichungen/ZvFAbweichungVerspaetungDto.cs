namespace BauFahrplanMonitor.Importer.Dto.ZvF.Abweichungen;

/// <summary>
/// Beschreibt eine Verspätungsregel im Rahmen einer ZvF-Abweichung.
///
/// Dieses DTO modelliert:
/// <list type="bullet">
///   <item>die Höhe der Verspätung</item>
///   <item>den räumlichen Beginn der Verspätungswirkung</item>
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
/// ZvFAbweichungVerspaetungDto      ← HIER
///   ↓
/// ZvFAbweichungDto (JsonRaw)
///   ↓
/// Upsert / Persistenz
/// </code>
///
/// Die fachliche Zuordnung zu:
/// <list type="bullet">
///   <item>Zugnummer</item>
///   <item>Verkehrstag</item>
///   <item>Abweichungsart</item>
/// </list>
/// erfolgt immer im übergeordneten <see cref="ZvFAbweichungDto"/>.
/// </remarks>
public class ZvFAbweichungVerspaetungDto {

    // ==========================================================
    // VERSPÄTUNG
    // ==========================================================

    /// <summary>
    /// Höhe der Verspätung in Minuten.
    ///
    /// Positive Werte bedeuten eine Verzögerung.
    /// </summary>
    public int Verspaetung { get; set; }

    // ==========================================================
    // WIRKUNGSBEGINN
    // ==========================================================

    /// <summary>
    /// RL100-/DS100-Code der Betriebsstelle,
    /// ab der die Verspätung wirksam ist.
    ///
    /// Kann <c>null</c> sein, wenn:
    /// <list type="bullet">
    ///   <item>die Verspätung global gilt</item>
    ///   <item>kein exakter Beginn angegeben ist</item>
    /// </list>
    /// </summary>
    public string? VerspaetungAbRl100 { get; set; }
}