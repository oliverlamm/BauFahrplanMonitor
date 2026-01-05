using System.Collections.Generic;

namespace BauFahrplanMonitor.Importer.Dto.BbpNeo;

/// <summary>
/// Domain-DTO für APS (Anlagenbezogene Sperrungen) im BBPNeo-Kontext.
///
/// Repräsentiert die Auswirkungen einer Regelung
/// auf technische Anlagen, Gleise oder betriebliche Einrichtungen.
/// </summary>
/// <remarks>
/// APS ist eine optionale, untergeordnete Struktur
/// innerhalb einer BVE und beschreibt:
///  - ob Anlagen betroffen sind
///  - welche Anlagen konkret betroffen sind
///  - welche betrieblichen Einschränkungen gelten
///
/// Architekturrolle:
/// <code>
/// BBPNeoMassnahme
///  └─ Regelung
///     └─ BVE
///        └─ APS          ← HIER
/// </code>
///
/// Dieses DTO ist:
///  - vollständig normalisiert
///  - frei von RAW-/XML-Abhängigkeiten
///  - direkt persistierbar
///
/// Die eigentliche fachliche Bewertung erfolgt
/// im Normalizer, nicht in diesem DTO.
/// </remarks>
public sealed class BbpNeoAps {

    /// <summary>
    /// Gibt an, ob grundsätzlich eine Anlagenbetroffenheit vorliegt.
    /// </summary>
    /// <remarks>
    /// <c>true</c> bedeutet, dass mindestens eine Anlage
    /// von der Regelung betroffen ist.
    /// </remarks>
    public bool Betroffenheit { get; init; }

    /// <summary>
    /// Freitext-Beschreibung der Anlagenbetroffenheit.
    /// </summary>
    /// <remarks>
    /// Kann erläuternde Hinweise enthalten,
    /// z. B. zur Art der Sperrung oder betroffenen Anlagen.
    /// </remarks>
    public string? Beschreibung { get; init; }

    /// <summary>
    /// Gibt an, ob der betroffene Bereich frei von Fahrzeugen sein muss.
    /// </summary>
    /// <remarks>
    /// <c>true</c>  → Bereich muss fahrzeugfrei sein  
    /// <c>false</c> → Fahrzeuge dürfen vorhanden sein  
    /// <c>null</c>  → keine Angabe im Quellsystem
    /// </remarks>
    public bool? FreiVonFahrzeugen { get; init; }

    /// <summary>
    /// Liste der konkret betroffenen Anlagen / Gleise.
    /// </summary>
    /// <remarks>
    /// Enthält eine oder mehrere
    /// <see cref="BbpNeoApsBetroffenheit"/>-Einträge,
    /// die die Auswirkungen im Detail beschreiben.
    ///
    /// Die Liste ist:
    ///  - leer, wenn keine Detailangaben vorhanden sind
    ///  - niemals <c>null</c>
    /// </remarks>
    public List<BbpNeoApsBetroffenheit> Betroffenheiten { get; init; } = [];
    
    public bool IstBetroffen { get; set; } = false;
}
