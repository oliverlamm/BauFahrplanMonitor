namespace BauFahrplanMonitor.Importer.Dto.ZvF.Abweichungen;

/// <summary>
/// Beschreibt einen einzelnen Haltausfall mit optionalem Ersatzhalt
/// im Rahmen einer ZvF-Abweichung.
///
/// Dieses DTO ist der kleinste fachliche Baustein
/// der Ersatzhalt-Logik.
///
/// Es enthält:
/// <list type="bullet">
///   <item>die Reihenfolge des Haltausfalls</item>
///   <item>den ausfallenden Halt</item>
///   <item>einen optionalen Ersatzhalt</item>
/// </list>
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
/// ZvFAbweichungHaltausfallDto      ← HIER
///   ↓
/// ZvFAbweichungErsatzhalteDto
///   ↓
/// ZvFAbweichungDto (JsonRaw)
/// </code>
///
/// Die Zuordnung zu:
/// <list type="bullet">
///   <item>Zugnummer</item>
///   <item>Verkehrstag</item>
///   <item>Abweichungsart</item>
/// </list>
/// erfolgt immer im übergeordneten Kontext.
/// </remarks>
public class ZvFAbweichungHaltausfallDto {
    // ==========================================================
    // REIHENFOLGE
    // ==========================================================

    /// <summary>
    /// Reihenfolge des Haltausfalls innerhalb der Abweichung.
    ///
    /// Entspricht typischerweise der Reihenfolge
    /// im ZvF-XML und kann fachlich relevant sein.
    /// </summary>
    public int Folge { get; set; }

    // ==========================================================
    // HALTANGABEN
    // ==========================================================

    /// <summary>
    /// RL100-/DS100-Code des ausfallenden Halts.
    /// </summary>
    public string? AusfallRl100 { get; set; }

    /// <summary>
    /// RL100-/DS100-Code des Ersatzhalts.
    ///
    /// Kann <c>null</c> sein, wenn der Halt ersatzlos entfällt.
    /// </summary>
    public string? ErsatzRl100 { get; set; }
}