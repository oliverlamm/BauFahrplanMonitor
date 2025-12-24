using BauFahrplanMonitor.Models;

namespace BauFahrplanMonitor.Importer.Dto.Shared;

public interface IExtendedVorgangDto {
    /// <summary>
    /// Überträgt importer-spezifische Felder
    /// auf die persistierte Vorgangsentität.
    /// </summary>
    void ApplyTo(UjbauVorgang entity);
    void ApplyIfEmptyTo(UjbauVorgang vorgang);
}