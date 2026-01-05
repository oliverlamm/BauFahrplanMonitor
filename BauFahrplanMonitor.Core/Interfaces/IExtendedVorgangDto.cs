using BauFahrplanMonitor.Models;

namespace BauFahrplanMonitor.Interfaces;

public interface IExtendedVorgangDto {
    /// <summary>
    /// Überträgt importer-spezifische Felder
    /// auf die persistierte Vorgangsentität.
    /// </summary>
    void ApplyTo(UjbauVorgang entity);
    void ApplyIfEmptyTo(UjbauVorgang vorgang);
}