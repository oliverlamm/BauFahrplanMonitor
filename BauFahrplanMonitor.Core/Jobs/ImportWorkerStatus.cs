namespace BauFahrplanMonitor.Core.Jobs;

public sealed class ImportWorkerStatus {
    public int    WorkerId { get; init; }
    public WorkerState State    { get; internal set; } = WorkerState.Idle;
    public string?   CurrentFile { get; internal set; }
    public DateTime? StartedAt   { get; internal set; }
}