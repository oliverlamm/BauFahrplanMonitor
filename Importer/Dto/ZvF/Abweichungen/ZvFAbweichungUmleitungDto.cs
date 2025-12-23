using System.Collections.Generic;

namespace BauFahrplanMonitor.Importer.Dto.ZvF.Abweichungen;

/// <summary>
/// Beschreibt eine Umleitungsregel im Rahmen einer ZvF-Abweichung.
///
/// Dieses DTO modelliert eine vollständige Umleitung und enthält:
/// <list type="bullet">
///   <item>eine textuelle Beschreibung der Umleitung</item>
///   <item>den konkreten Umleitungsweg als Liste von Betriebsstellen</item>
///   <item>die prognostizierte Verspätung durch die Umleitung</item>
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
/// ZvFAbweichungUmleitungDto       ← HIER
///   ↓
/// ZvFAbweichungDto (JsonRaw)
///   ↓
/// Upsert / Persistenz
/// </code>
///
/// Der Bezug zu:
/// <list type="bullet">
///   <item>Zugnummer</item>
///   <item>Verkehrstag</item>
///   <item>Abweichungsart</item>
/// </list>
/// erfolgt immer über das übergeordnete <see cref="ZvFAbweichungDto"/>.
/// </remarks>
public class ZvFAbweichungUmleitungDto {

    // ==========================================================
    // BESCHREIBUNG
    // ==========================================================

    /// <summary>
    /// Textuelle Beschreibung der Umleitung.
    ///
    /// Entspricht in der Regel dem Umleitungstext
    /// aus dem ZvF-XML.
    /// </summary>
    public string Umleitung { get; set; } = string.Empty;

    // ==========================================================
    // UMLEITUNGSWEG
    // ==========================================================

    /// <summary>
    /// Umleitungsweg als Liste von RL100-/DS100-Codes.
    ///
    /// Die Reihenfolge ist fachlich relevant
    /// und beschreibt den tatsächlichen Fahrweg
    /// der Umleitung.
    /// </summary>
    public List<string> UmleitwegRl100 { get; set; } = [];

    // ==========================================================
    // FOLGEWIRKUNG
    // ==========================================================

    /// <summary>
    /// Prognostizierte Verspätung in Minuten,
    /// die durch die Umleitung entsteht.
    /// </summary>
    public int PrognostizierteVerspaetung { get; set; }
}
