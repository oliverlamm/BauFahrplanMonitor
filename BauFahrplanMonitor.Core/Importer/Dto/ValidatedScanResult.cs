using BauFahrplanMonitor.Helpers;
using BauFahrplanMonitor.Importer.Dto;

namespace BauFahrplanMonitor.Core.Importer.Dto;

public sealed class ValidatedScanResult {
    public List<ImportFileItem> Queue { get; } = new();
    public ScanStat             Stat  { get; } = new();
}