namespace BauFahrplanMonitor.Core.Helpers;

public sealed class ZvFExportScanItem {
    public string     FilePath { get; init; } = null!;
    public ImportMode Mode     { get; init; }

    public string?   DokumentId { get; init; }
    public string?   Region     { get; init; }
    public DateOnly? GueltigAb  { get; init; }
    public DateOnly? GueltigBis { get; init; }

    public bool    ExistsInDatabase { get; init; }
    public bool    IsImportable     { get; init; }
    public string? SkipReason       { get; init; }
}