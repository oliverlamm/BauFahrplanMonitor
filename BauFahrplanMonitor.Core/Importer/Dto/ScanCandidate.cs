using BauFahrplanMonitor.Core.Helpers;

namespace BauFahrplanMonitor.Core.Importer.Dto;

public sealed class ScanCandidate {
    public string     FilePath { get; init; } = null!;
    public ImportMode Mode     { get; init; }
}