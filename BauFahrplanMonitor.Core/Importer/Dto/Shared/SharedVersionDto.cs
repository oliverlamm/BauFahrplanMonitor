namespace BauFahrplanMonitor.Core.Importer.Dto.Shared;

/// <summary>
/// Gemeinsames Versions-DTO für importierte Dokumente.
///
/// Kapselt Versionsinformationen in strukturierter Form
/// und ermöglicht einfache Vergleiche zwischen Dokumentständen.
/// </summary>
/// <remarks>
/// Dieses DTO wird importerübergreifend verwendet
/// (ZvF, ÜB, FPLO, BBPNeo, …).
///
/// Die Version kann:
///  - explizit aus dem Dokument stammen
///  - implizit aus Metadaten abgeleitet werden
///
/// Durch die getrennten Felder ist eine
/// fein granulare Auswertung möglich,
/// während <see cref="VersionNumeric"/> einfache
/// Vergleiche erlaubt.
/// </remarks>
public class SharedVersionDto {

    /// <summary>
    /// Hauptversion (Major).
    /// </summary>
    /// <remarks>
    /// Erhöht sich bei grundlegenden Änderungen
    /// am Dokument oder Datenformat.
    /// </remarks>
    public int Major { get; set; }

    /// <summary>
    /// Nebenversion (Minor).
    /// </summary>
    /// <remarks>
    /// Erhöht sich bei funktionalen Erweiterungen
    /// ohne grundlegende Strukturänderung.
    /// </remarks>
    public int Minor { get; set; }

    /// <summary>
    /// Unterversion / Patch-Stand.
    /// </summary>
    /// <remarks>
    /// Wird typischerweise für:
    ///  - kleinere Korrekturen
    ///  - redaktionelle Anpassungen
    /// verwendet.
    /// </remarks>
    public int Sub { get; set; }

    /// <summary>
    /// Numerische Repräsentation der Version.
    /// </summary>
    /// <remarks>
    /// Optionales Feld zur schnellen Vergleichbarkeit
    /// von Versionen (z. B. in der Datenbank).
    ///
    /// Beispiel:
    /// <code>
    /// Major=1, Minor=2, Sub=3 → VersionNumeric=1002003
    /// </code>
    ///
    /// Die genaue Berechnungslogik wird
    /// bewusst außerhalb dieses DTOs gehalten.
    /// </remarks>
    public long? VersionNumeric { get; set; }
}