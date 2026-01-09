namespace BauFahrplanMonitor.Core.Jobs;

public enum WorkerState {
    Idle,
    Working,
    Stopping,
    Error,
    Canceled
}