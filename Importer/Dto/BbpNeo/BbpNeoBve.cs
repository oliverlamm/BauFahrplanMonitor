using System;
using BauFahrplanMonitor.Importer.Upsert;

namespace BauFahrplanMonitor.Importer.Dto.BbpNeo;

/// <summary>
/// Domain-DTO für eine BVE (Betriebliche Verfahrensanordnung)
/// im BBPNeo-Kontext.
///
/// Eine BVE beschreibt die konkrete betriebliche Umsetzung
/// einer Regelung, inklusive Gültigkeit, Lage und optionaler
/// Auswirkungen auf Anlagen (APS) und Industrieanschlüsse (IAV).
/// </summary>
/// <remarks>
/// Architekturrolle:
/// <code>
/// BbpNeoMassnahme
///  └─ Regelung
///     └─ BVE                 ← HIER
///        ├─ APS (optional)
///        └─ IAV (optional)
/// </code>
///
/// Dieses DTO ist:
///  - vollständig normalisiert
///  - frei von XML-/RAW-Abhängigkeiten
///  - DB-nah, aber kein EF-Entity
///
/// Die eigentliche Persistenz erfolgt
/// im <see cref="BbpNeoUpsertService"/>.
/// </remarks>
public sealed class BbpNeoBve {

    // ==========================================================
    // IDENTITÄT
    // ==========================================================

    /// <summary>
    /// Eindeutige BVE-ID.
    /// </summary>
    /// <remarks>
    /// Fachlicher Primärschlüssel der BVE
    /// innerhalb einer Regelung.
    ///
    /// Wird im Upsert als zwingend erforderlich betrachtet.
    /// </remarks>
    public required string BveId { get; init; }

    /// <summary>
    /// Gibt an, ob die BVE aktiv ist.
    /// </summary>
    public bool Aktiv { get; init; }

    // ==========================================================
    // BASISINFORMATIONEN
    // ==========================================================

    /// <summary>
    /// Art der BVE.
    /// </summary>
    /// <remarks>
    /// Fachliche Klassifikation
    /// (z. B. Sperrung, Einschränkung, Umleitung).
    /// </remarks>
    public string? Art { get; init; }

    /// <summary>
    /// Mikroskopische Ortsbeschreibung.
    /// </summary>
    /// <remarks>
    /// Dient zur präzisen räumlichen Einordnung
    /// innerhalb einer Betriebsstelle oder Strecke.
    /// </remarks>
    public string? OrtMikroskopisch { get; init; }

    /// <summary>
    /// Freitext-Bemerkung zur BVE.
    /// </summary>
    public string? Bemerkung { get; init; }

    // ==========================================================
    // GÜLTIGKEIT (ZEITLICH)
    // ==========================================================

    /// <summary>
    /// Gültigkeitsbeschreibung (DB-nah).
    /// </summary>
    /// <remarks>
    /// Freitext oder strukturierter Text,
    /// wie er im Quellsystem vorliegt.
    /// </remarks>
    public string? Gueltigkeit { get; init; }

    /// <summary>
    /// Effektive Verkehrstage der Gültigkeit.
    /// </summary>
    /// <remarks>
    /// Enthält ggf. eine Bitmasken- oder Textbeschreibung
    /// der tatsächlich betroffenen Verkehrstage.
    /// </remarks>
    public string? GueltigkeitEffektiveVerkehrstage { get; init; }

    /// <summary>
    /// Beginn der Gültigkeit.
    /// </summary>
    public DateTime? GueltigkeitVon { get; init; }

    /// <summary>
    /// Ende der Gültigkeit.
    /// </summary>
    public DateTime? GueltigkeitBis { get; init; }

    // ==========================================================
    // LAGE (FACHLICH)
    // ==========================================================

    /// <summary>
    /// DS100-Code der Abgangs-Betriebsstelle.
    /// </summary>
    /// <remarks>
    /// Wird später über Resolver
    /// in eine Datenbank-Referenz überführt.
    /// </remarks>
    public string? VonBstDs100 { get; init; }

    /// <summary>
    /// DS100-Code der Ziel-Betriebsstelle.
    /// </summary>
    public string? BisBstDs100 { get; init; }

    /// <summary>
    /// VzG-Nummer der Startstrecke.
    /// </summary>
    public long? VonVzG { get; init; }

    /// <summary>
    /// VzG-Nummer der Zielstrecke.
    /// </summary>
    public long? BisVzG { get; init; }

    // ==========================================================
    // OPTIONALE CHILD-STRUKTUREN
    // ==========================================================

    /// <summary>
    /// Anlagenbezogene Sperrungen (APS).
    /// </summary>
    /// <remarks>
    /// Optional; nur vorhanden, wenn Anlagen
    /// von der BVE betroffen sind.
    /// </remarks>
    public BbpNeoAps? Aps { get; init; }

    /// <summary>
    /// Industrieanschluss-Verkehre (IAV).
    /// </summary>
    /// <remarks>
    /// Optional; nur vorhanden, wenn
    /// Industrieanschlüsse betroffen sind.
    /// </remarks>
    public BbpNeoIav? Iav { get; init; }
}
