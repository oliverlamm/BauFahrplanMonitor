using BauFahrplanMonitor.Core.Importer.Dto;

namespace BauFahrplanMonitor.Core.Jobs;

public sealed class ZvFExportJobStatus {
    public           ImportJobState                    State   { get; internal set; } = ImportJobState.Idle;
    public           IReadOnlyList<ImportWorkerStatus> Workers => _workers;
    private readonly ImportWorkerStatus[]              _workers;
    // Scan-Statistik (euer typ-spezifisches Modell)
    public ScanStat ScanStat { get; set; } = new();

    // nur Anzahl, nicht die ganze Queue (API freundlich)
    public int     QueueCount  { get; set; }
    public int     TotalFiles  { get; internal set; }
    public string? CurrentFile { get; internal set; }

    public           int  ActiveWorkers => _activeWorkers;
    private volatile bool _softCancelRequested;

    public int Errors => _errors;

    public DateTime? StartedAt { get; internal set; }

    public int ProcessedFiles => _processedFiles;

    private int _processedFiles;
    private int _activeWorkers;
    private int _errors;
    internal void IncrementProcessedFiles()
        => Interlocked.Increment(ref _processedFiles);

    internal void IncrementActiveWorkers()
        => Interlocked.Increment(ref _activeWorkers);

    internal void DecrementActiveWorkers()
        => Interlocked.Decrement(ref _activeWorkers);

    internal void ResetActiveWorkers()
        => Interlocked.Exchange(ref _activeWorkers, 0);

    internal void IncrementErrors()
        => Interlocked.Increment(ref _errors);

    internal void ResetErrors()
        => Interlocked.Exchange(ref _errors, 0);

    internal void ResetProcessedFiles()
        => Interlocked.Exchange(ref _processedFiles, 0);

    public ZvFExportJobStatus(int workerCount) {
        _workers = Enumerable
            .Range(1, workerCount)
            .Select(i => new ImportWorkerStatus {
                WorkerId = i
            })
            .ToArray();
    }

    internal ImportWorkerStatus AcquireWorker() {
        lock (_workers) {
            var worker = _workers.First(w => w.State == WorkerState.Idle);
            worker.State     = WorkerState.Working;
            worker.StartedAt = DateTime.Now;
            return worker;
        }
    }

    internal void ReleaseWorker(ImportWorkerStatus worker) {
        lock (_workers) {
            worker.State       = WorkerState.Idle;
            worker.CurrentFile = null;
            worker.StartedAt   = null;
        }
    }
}