namespace BauFahrplanMonitor.Core.Jobs;

public enum ImportJobState {
    Idle,
    Scanning,
    Scanned,
    Running,
    Finished,
    Aborted,
    Failed,
    FinishedWithErrors
}
