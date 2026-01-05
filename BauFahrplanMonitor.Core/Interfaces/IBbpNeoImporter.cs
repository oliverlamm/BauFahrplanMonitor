using System.Threading;
using System.Threading.Tasks;
using BauFahrplanMonitor.Importer.Dto.BbpNeo;

namespace BauFahrplanMonitor.Interfaces;

/// <summary>
/// Erweiterung für BBPNeo-Importer:
/// erlaubt Header-Lesen vor dem eigentlichen Import.
/// </summary>
public interface IBbpNeoImporter : IFileImporter {

    /// <summary>
    /// Liest ausschließlich den Header der BBPNeo-Datei
    /// (ohne Persistenz, ohne Queue, ohne Worker).
    /// </summary>
    Task<BbpNeoHeaderInfo> ReadHeaderAsync(
        string            filePath,
        CancellationToken token = default);
}