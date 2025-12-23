using System.Collections.Generic;

namespace BauFahrplanMonitor.Importer.Dto.ZvF.Abweichungen;

/// <summary>
/// Beschreibt eine Menge von Ersatzhalten bzw. Haltausfällen
/// im Rahmen einer ZvF-Abweichung.
///
/// Dieses DTO fasst mehrere einzelne Haltausfall-Regeln
/// zu einer strukturierten Liste zusammen.
///
/// Es handelt sich um:
/// <list type="bullet">
///   <item>ein Regelungs-Teil-DTO</item>
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
/// ZvFAbweichungErsatzhalteDto      ← HIER
///   ↓
/// ZvFAbweichungDto (JsonRaw)
///   ↓
/// Upsert / Persistenz
/// </code>
///
/// Die eigentliche fachliche Zuordnung zu:
/// <list type="bullet">
///   <item>Zugnummer</item>
///   <item>Verkehrstag</item>
///   <item>Abweichungsart</item>
/// </list>
/// erfolgt immer im übergeordneten <see cref="ZvFAbweichungDto"/>.
/// </remarks>
public class ZvFAbweichungErsatzhalteDto {

    // ==========================================================
    // HALTELISTE
    // ==========================================================

    /// <summary>
    /// Liste der einzelnen Haltausfall- bzw. Ersatzhalt-Regeln.
    ///
    /// Jeder Eintrag beschreibt:
    /// <list type="bullet">
    ///   <item>einen ausfallenden Halt</item>
    ///   <item>und ggf. einen zugehörigen Ersatzhalt</item>
    /// </list>
    ///
    /// Die Reihenfolge entspricht der Struktur im ZvF-XML
    /// und kann fachlich relevant sein.
    /// </summary>
    public List<ZvFAbweichungHaltausfallDto> Halteliste { get; set; } = [];
}