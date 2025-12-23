using System;
using System.Collections.Generic;

namespace BauFahrplanMonitor.Importer.Dto.Shared;

/// <summary>
/// Gemeinsames Basis-Dokument-DTO für alle Importer.
///
/// Repräsentiert importerübergreifende Metadaten eines Dokuments
/// (ZvF, ÜB, FPLO, BBPNeo, …).
///
/// Verantwortlich für:
///  - Zentrale Dokument-Metadaten
///  - Sammlung der zugehörigen Strecken
///  - Versionsinformationen
///  - Allgemeine, nicht strukturierte Texte
///
/// NICHT verantwortlich für:
///  - Importer-spezifische Detaildaten
///  - Normalisierung oder Business-Logik
///  - Persistenz oder Datenbanklogik
/// </summary>
/// <remarks>
/// Dieses DTO bildet die **gemeinsame Schnittmenge**
/// aller importierten Dokumenttypen.
///
/// Es wird typischerweise:
///  - im Normalizer aufgebaut
///  - von BusinessLogic weiterverarbeitet
///  - im Dokument-Upsert persistiert
///
/// Importer-spezifische DTOs erweitern dieses Modell
/// entweder direkt oder über Komposition.
/// </remarks>
public class SharedDocumentDto {
    /// <summary>
    /// Masterniederlassung des Dokuments.
    /// </summary>
    /// <remarks>
    /// Bezeichnet die organisatorische Einheit
    /// (z. B. Niederlassung / Bereich),
    /// die für das Dokument federführend ist.
    /// </remarks>
    public string Masterniederlassung { get; set; } = "";

    /// <summary>
    /// Zeitpunkt, zu dem das Dokument exportiert wurde.
    /// </summary>
    /// <remarks>
    /// Dieser Zeitstempel stammt typischerweise:
    ///  - aus dem XML
    ///  - aus Metadaten
    ///  - oder aus dem Dateinamen
    ///
    /// Er dient u. a. zur:
    ///  - Versionsbewertung
    ///  - Dublettenvermeidung
    ///  - Nachvollziehbarkeit von Importen
    /// </remarks>
    public DateTime ExportTimestamp { get; set; }

    /// <summary>
    /// Liste der im Dokument enthaltenen Strecken.
    /// </summary>
    /// <remarks>
    /// Enthält alle betroffenen Strecken
    /// inklusive ihrer Metadaten.
    ///
    /// Die Reihenfolge ist fachlich
    /// nicht zwingend relevant.
    /// </remarks>
    public List<SharedStreckeDto> Strecken { get; set; } = new();

    /// <summary>
    /// Versionsinformationen des Dokuments.
    /// </summary>
    /// <remarks>
    /// Enthält Angaben zur:
    ///  - Dokumentversion
    ///  - ggf. System- oder Exportversion
    ///
    /// Wird zentral gehalten, um
    /// Versionslogik importerübergreifend
    /// konsistent auszuwerten.
    /// </remarks>
    public SharedVersionDto Version { get; set; } = new();

    /// <summary>
    /// Allgemeiner, freier Text zum Dokument.
    /// </summary>
    /// <remarks>
    /// Wird verwendet für:
    ///  - Hinweise
    ///  - Bemerkungen
    ///  - nicht weiter strukturierte Zusatzinformationen
    ///
    /// Der Inhalt wird bewusst nicht
    /// weiter interpretiert.
    /// </remarks>
    public string AllgemeinText { get; set; } = "";
}