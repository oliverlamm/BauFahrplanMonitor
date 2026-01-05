using BauFahrplanMonitor.Importer.Dto;

namespace BauFahrplanMonitor.Core.Jobs;

public sealed class ZvFExportJobStatus {
    public ImportJobState State { get; internal set; } = ImportJobState.Idle;
    
    // Scan-Statistik (euer typ-spezifisches Modell)
    public ScanStat ScanStat { get; set; } = new();

    // nur Anzahl, nicht die ganze Queue (API freundlich)
    public int     QueueCount  { get; set; }
    public int     TotalFiles  { get; internal set; }
    public string? CurrentFile { get; internal set; }

    public int ActiveWorkers => _activeWorkers;

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
}

