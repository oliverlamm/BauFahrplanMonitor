using System;
using System.Collections.Generic;

namespace BauFahrplanMonitor.Importer.Dto.Shared;

/// <summary>
/// Gemeinsames Header-DTO für alle Importer (ZvF, ÜB, FPLO, …).
///
/// Enthält Metadaten zur Importdatei selbst sowie
/// Absenderinformationen des erzeugenden Systems
/// oder der verantwortlichen Person.
/// </summary>
/// <remarks>
/// Dieses DTO ist importerübergreifend identisch
/// und bildet den „Kopf“ eines importierten Dokuments.
///
/// Typische Verwendung:
///  - wird beim XML-Parsing / Normalizing befüllt
///  - dient als Quelle für Sender-Upserts
///  - wird für Logging, Nachvollziehbarkeit und Audits genutzt
///
/// Das DTO enthält bewusst **keine fachlichen Inhalte**
/// (Züge, Strecken, Maßnahmen etc.).
/// </remarks>
public class SharedHeaderDto {
    /// <summary>
    /// Zeitstempel des Dokuments.
    /// </summary>
    /// <remarks>
    /// Repräsentiert in der Regel:
    ///  - den Erstellungszeitpunkt
    ///  - oder den Exportzeitpunkt des Dokuments
    ///
    /// Wird u. a. verwendet für:
    ///  - Versionierung
    ///  - Fahrplanjahr-Bestimmung
    ///  - Logging
    /// </remarks>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Dateiname der Importdatei (ohne Pfad).
    /// </summary>
    /// <remarks>
    /// Wird primär für:
    ///  - UI-Anzeige
    ///  - Logging
    ///  - Nachvollziehbarkeit
    /// verwendet.
    /// </remarks>
    public string? FileName { get; set; }

    // ---------------------------------------------------------
    // ABSENDERDATEN
    // ---------------------------------------------------------

    /// <summary>
    /// Nachname des Absenders.
    /// </summary>
    public string SenderName { get; set; } = "";

    /// <summary>
    /// Vorname des Absenders.
    /// </summary>
    public string SenderVorname { get; set; } = "";

    /// <summary>
    /// E-Mail-Adresse des Absenders.
    /// </summary>
    /// <remarks>
    /// Wird typischerweise verwendet zur:
    ///  - eindeutigen Identifikation eines Senders
    ///  - Auflösung / Anlage über den SharedSenderUpsertService
    /// </remarks>
    public string SenderMail { get; set; } = "";

    /// <summary>
    /// Abteilung des Absenders.
    /// </summary>
    public string SenderAbteilung { get; set; } = "";

    /// <summary>
    /// Telefonnummer des Absenders.
    /// </summary>
    public string SenderTelefon { get; set; } = "";

    /// <summary>
    /// Stadt des Absenders.
    /// </summary>
    public string SenderStadt { get; set; } = "";

    /// <summary>
    /// Postleitzahl des Absenders.
    /// </summary>
    /// <remarks>
    /// Als <c>int</c> modelliert, da
    /// führende Nullen im DB-Kontext
    /// i. d. R. nicht relevant sind.
    /// </remarks>
    public int SenderPlz { get; set; } = 0;

    /// <summary>
    /// Adresse (Straße, Hausnummer) des Absenders.
    /// </summary>
    public string SenderAdresse { get;    set; } = "";

    public List<string> Empfaenger { get; set; } = [];
}