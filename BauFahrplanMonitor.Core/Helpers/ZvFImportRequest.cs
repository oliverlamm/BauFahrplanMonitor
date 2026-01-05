namespace BauFahrplanMonitor.Core.Helpers;

public sealed class ZvFImportRequest {
    public ZvFImportCommand Command { get; init; }
    public ZvFFileFilter    Filter  { get; init; }
}
