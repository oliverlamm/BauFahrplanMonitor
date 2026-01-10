using BauFahrplanMonitor.Core.Importer.Dto;

namespace BauFahrplanMonitor.Core.Jobs;

public sealed class ZvFExportJobStatus {

    // =========================================================
    // Phase
    // =========================================================
    public ImportJobState State { get; internal set; } = ImportJobState.Idle;

    // =========================================================
    // Scan-Fortschritt (Dateien)
    // =========================================================
    public int ScanTotalFiles     { get; internal set; }
    public int ScanProcessedFiles => _scanProcessedFiles;

    // =========================================================
    // Import-Fortschritt (Queue-Items)
    // =========================================================
    public int ImportTotalItems     { get; internal set; }
    public int ImportProcessedItems => _importProcessedItems;
    public int ImportErrorItems     => _errors;

    // =========================================================
    // Threads / Worker
    // =========================================================
    public IReadOnlyList<ImportWorkerStatus> Workers => _workers;
    public int ActiveWorkers => _activeWorkers;

    // =========================================================
    // Anzeige / Meta
    // =========================================================
    public string? CurrentFile { get; internal set; }
    public DateTime? StartedAt { get; internal set; }

    // =========================================================
    // Scan-Statistik (bestehendes Modell)
    // =========================================================
    public ScanStat ScanStat { get; internal set; } = new();

    // =========================================================
    // Backing Fields (thread-safe)
    // =========================================================
    private readonly ImportWorkerStatus[] _workers;

    private int _scanProcessedFiles;
    private int _importProcessedItems;
    private int _activeWorkers;
    private int _errors;

    // =========================================================
    // ctor
    // =========================================================
    public ZvFExportJobStatus(int workerCount) {
        _workers = Enumerable
            .Range(1, workerCount)
            .Select(i => new ImportWorkerStatus {
                WorkerId = i
            })
            .ToArray();
    }

    // =========================================================
    // Scan-Zähler
    // =========================================================
    internal void ResetScan(int totalFiles) {
        ScanTotalFiles = totalFiles;
        Interlocked.Exchange(ref _scanProcessedFiles, 0);
    }

    internal void IncrementScanProcessed() {
        Interlocked.Increment(ref _scanProcessedFiles);
    }

    // =========================================================
    // Import-Zähler
    // =========================================================
    internal void ResetImport(int totalItems) {
        ImportTotalItems = totalItems;
        Interlocked.Exchange(ref _importProcessedItems, 0);
        Interlocked.Exchange(ref _errors, 0);
        StartedAt = DateTime.Now;
    }

    internal void IncrementImportProcessed() {
        Interlocked.Increment(ref _importProcessedItems);
    }

    internal void IncrementImportErrors() {
        Interlocked.Increment(ref _errors);
    }

    // =========================================================
    // Worker-Tracking
    // =========================================================
    internal void ResetActiveWorkers() {
        Interlocked.Exchange(ref _activeWorkers, 0);
    }

    internal ImportWorkerStatus AcquireWorker() {
        lock (_workers) {
            var worker = _workers.First(w => w.State == WorkerState.Idle);
            worker.State     = WorkerState.Working;
            worker.StartedAt = DateTime.Now;

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
    
    internal void UpdateWorkerProgress(
        ImportWorkerStatus worker,
        string             message) {

        lock (_workers) {
            worker.ProgressMessage = message;
        }
    }

    internal void ClearWorkerProgress(ImportWorkerStatus worker) {
        lock (_workers) {
            worker.ProgressMessage = null;
        }
    }
}
