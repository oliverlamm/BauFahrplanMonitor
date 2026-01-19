namespace BauFahrplanMonitor.Core.Jobs;

public sealed class NetzfahrplanJobStatus {
    // ----------------------------
    // Job-Level Status
    // ----------------------------
    public ImportJobState State { get; internal set; } = ImportJobState.Idle;

    public int TotalFiles { get; internal set; }
    public int QueueCount { get; internal set; }

    public int ProcessedFiles => _processedFiles;
    public int Errors         => _errors;

    public DateTime? StartedAt { get; internal set; }

    // ----------------------------
    // Worker-Status
    // ----------------------------
    public IReadOnlyList<ImportWorkerStatus> Workers => _workers;

    public int ActiveWorkers => _activeWorkers;

    // ----------------------------
    // Intern
    // ----------------------------
    private readonly ImportWorkerStatus[] _workers;

    private int _processedFiles;
    private int _errors;
    private int _activeWorkers;

    // ----------------------------
    // Ctor
    // ----------------------------
    public NetzfahrplanJobStatus(int workerCount) {
        if (workerCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(workerCount));

        _workers = Enumerable
            .Range(1, workerCount)
            .Select(i => new ImportWorkerStatus {
                WorkerId = i
            })
            .ToArray();
    }

    // ----------------------------
    // Worker Handling
    // ----------------------------
    internal ImportWorkerStatus AcquireWorker() {
        lock (_workers) {
            var worker = _workers.First(w => w.State == WorkerState.Idle);
            worker.State     = WorkerState.Working;
            worker.StartedAt = DateTime.UtcNow;
            Interlocked.Increment(ref _activeWorkers);
            return worker;
        }
    }

    internal void ReleaseWorker(ImportWorkerStatus worker) {
        lock (_workers) {
            worker.State       = WorkerState.Idle;
            worker.CurrentFile = null;
            worker.StartedAt   = null;
            Interlocked.Decrement(ref _activeWorkers);
        }
    }

    // ----------------------------
    // Counters
    // ----------------------------
    internal void IncrementProcessed()
        => Interlocked.Increment(ref _processedFiles);

    internal void IncrementErrors()
        => Interlocked.Increment(ref _errors);

    // ----------------------------
    // Reset (z. B. bei neuem Scan)
    // ----------------------------
    internal void Reset() {
        _processedFiles = 0;
        _errors         = 0;
        _activeWorkers  = 0;

        foreach (var w in _workers) {
            w.State       = WorkerState.Idle;
            w.CurrentFile = null;
            w.StartedAt   = null;
        }

        TotalFiles = 0;
        QueueCount = 0;
        StartedAt  = null;
        State      = ImportJobState.Idle;
    }

    internal void IncrementActiveWorkers()
        => Interlocked.Increment(ref _activeWorkers);

    internal void DecrementActiveWorkers()
        => Interlocked.Decrement(ref _activeWorkers);

    internal void IncrementProcessedFiles()
        => Interlocked.Increment(ref _processedFiles);

    public void DecrementQueueCount() {
        QueueCount = Math.Max(0, QueueCount - 1);
    }

}