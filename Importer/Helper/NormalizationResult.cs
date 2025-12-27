using System.Collections.Generic;

namespace BauFahrplanMonitor.Importer.Mapper;

/// <summary>
/// Ergebniscontainer für Normalisierungsprozesse.
///
/// Kapselt:
///  - den erfolgreich normalisierten Wert
///  - eine Liste nicht-fataler Warnungen
///
/// Wird verwendet, um fehlerhafte oder unvollständige
/// Eingangsdaten kontrolliert weiterzuverarbeiten,
/// ohne den Import abbrechen zu müssen.
/// </summary>
/// <typeparam name="T">
/// Typ des normalisierten Domain-Objekts
/// (z. B. <c>BbpNeoMassnahme</c>).
/// </typeparam>
/// <remarks>
/// Dieses Pattern wird bewusst verwendet statt:
///  - Exceptions bei fachlichen Problemen
///  - stilles Ignorieren von Inkonsistenzen
///
/// Architekturprinzip:
/// <list type="bullet">
///   <item>Parsing: strukturell korrekt oder Abbruch</item>
///   <item>Normalisierung: tolerant, warnungsbasiert</item>
///   <item>Persistenz: entscheidet über Konsequenzen</item>
/// </list>
///
/// Dadurch bleibt der Import:
///  - robust
///  - nachvollziehbar
///  - auditierbar
/// </remarks>
public sealed class NormalizationResult<T> {

    /// <summary>
    /// Das normalisierte Domain-Objekt.
    /// </summary>
    /// <remarks>
    /// Muss immer gesetzt sein.
    /// Wird per <c>required</c> erzwungen,
    /// um inkonsistente Normalisierungsergebnisse
    /// zur Compile-Zeit zu verhindern.
    /// </remarks>
    public required T Value { get; init; }

    /// <summary>
    /// Sammlung nicht-fataler Warnungen aus dem Normalizer.
    /// </summary>
    /// <remarks>
    /// Enthält:
    ///  - ungültige Einzelwerte
    ///  - fehlende optionale Felder
    ///  - fachliche Inkonsistenzen
    ///
    /// Die Warnungen werden typischerweise:
    ///  - geloggt
    ///  - im UI angezeigt
    ///  - oder bei der Persistenz bewertet
    /// </remarks>
    public List<string> Warnings { get; set; } = [];

    /// <summary>
    /// Gibt an, ob Warnungen vorhanden sind.
    /// </summary>
    /// <remarks>
    /// Komforteigenschaft für:
    ///  - Logging
    ///  - UI-Status
    ///  - Filterlogik
    /// </remarks>
    public bool HasWarnings => Warnings.Count > 0;
}
