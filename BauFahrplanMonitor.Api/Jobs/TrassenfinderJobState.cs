namespace BauFahrplanMonitor.Api.Jobs;

public enum TrassenfinderJobState {
    Pending = 0,
    Running = 1,
    Done    = 2,
    Failed  = 3
}