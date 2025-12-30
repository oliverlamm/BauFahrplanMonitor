using BauFahrplanMonitor.Importer.Helper;
using BauFahrplanMonitor.Interfaces;
using BauFahrplanMonitor.Models;

namespace BauFahrplanMonitor.Importer.Dto.Shared;

/// <summary>
/// Gemeinsames Vorgangs-DTO für alle Importer.
///
/// Repräsentiert die fachlichen Kerndaten
/// eines Vorgangs, unabhängig vom konkreten Dokumenttyp.
/// </summary>
/// <remarks>
/// Dieses DTO bildet die Brücke zwischen:
///  - importierten Dokumenten (ZvF, ÜB, FPLO, …)
///  - und dem persistierten <c>ujbau_vorgang</c>-Datensatz.
///
/// Es enthält ausschließlich die **fachlich relevanten Schlüssel**
/// zur Vorgangsermittlung und -anlage.
/// </remarks>
public class SharedVorgangDto  : IExtendedVorgangDto{

    /// <summary>
    /// Master-FPLO-Nummer des Vorgangs.
    /// </summary>
    /// <remarks>
    /// Dient als primärer fachlicher Schlüssel
    /// für die Zuordnung eines Dokuments zu einem Vorgang.
    ///
    /// Wird in Kombination mit <see cref="FahrplanJahr"/>
    /// zur eindeutigen Identifikation verwendet.
    /// </remarks>
    public long MasterFplo { get; set; }

    /// <summary>
    /// Kategorie des Vorgangs.
    /// </summary>
    /// <remarks>
    /// Kennzeichnet die fachliche Einordnung
    /// des Vorgangs (z. B. A, B, C).
    ///
    /// Der Defaultwert <c>"A"</c> entspricht
    /// der Standardkategorie.
    /// </remarks>
    public string Kategorie { get; set; } = "";

    /// <summary>
    /// Fahrplanjahr des Vorgangs.
    /// </summary>
    /// <remarks>
    /// Wird typischerweise:
    ///  - aus Dokumentdaten abgeleitet
    ///  - über <see cref="FahrplanjahrHelper"/>
    /// bestimmt
    ///
    /// Kann <c>null</c> sein, wenn:
    ///  - keine eindeutige Zuordnung möglich ist
    ///  - Legacy-Daten importiert werden
    /// </remarks>
    public int? FahrplanJahr { get; set; }
    
    public void ApplyTo(UjbauVorgang v) {
        v.Kategorie = Kategorie;
    }

    public void ApplyIfEmptyTo(UjbauVorgang v) {
        if (string.IsNullOrWhiteSpace(v.Kategorie)
            && !string.IsNullOrWhiteSpace(Kategorie)) {
            v.Kategorie = Kategorie;
        }
    }
}