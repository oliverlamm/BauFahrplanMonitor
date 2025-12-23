namespace BauFahrplanMonitor.Importer.Dto.ZvF.Abweichungen;

/// <summary>
/// Beschreibt eine Vorplan-Abweichung im Rahmen einer ZvF-Abweichung.
///
/// Eine Vorplan-Abweichung gibt an,
/// dass der Zug ab einem bestimmten Punkt
/// gegenüber dem Regel-Fahrplan vorgezogen fährt.
///
/// Dieses DTO modelliert:
/// <list type="bullet">
///   <item>den Umfang der Vorverlegung (Vorplan)</item>
///   <item>den räumlichen Beginn der Wirkung</item>
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
/// ZvFAbweichungVorplanDto        ← HIER
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
public class ZvFAbweichungVorplanDto {

    // ==========================================================
    // VORPLAN
    // ==========================================================

    /// <summary>
    /// Umfang der Vorverlegung (Vorplan).
    ///
    /// Typischerweise als Minuten angegeben.
    /// Positive Werte bedeuten:
    /// <list type="bullet">
    ///   <item>der Zug fährt früher als geplant</item>
    /// </list>
    /// </summary>
    public int Vorplan { get; set; }

    // ==========================================================
    // WIRKUNGSBEGINN
    // ==========================================================

    /// <summary>
    /// RL100-/DS100-Code der Betriebsstelle,
    /// ab der der Vorplan wirksam ist.
    ///
    /// Kann <c>null</c> sein, wenn:
    /// <list type="bullet">
    ///   <item>die Vorverlegung global gilt</item>
    ///   <item>kein exakter Beginn angegeben ist</item>
    /// </list>
    /// </summary>
    public string? VorplanAbRl100 { get; set; }
}
