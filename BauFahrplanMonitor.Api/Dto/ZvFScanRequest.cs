using BauFahrplanMonitor.Core.Helpers;

public sealed class ZvFScanRequest {
    public ZvFFileFilter Filter { get; init; } = ZvFFileFilter.All;
}