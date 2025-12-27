namespace BauFahrplanMonitor.Importer.Helper;

public sealed class ImportProgressInfo {
    public string FileName { get; init; } = "";
    public string Step     { get; init; } = "";
    public int?   SubIndex { get; init; }
    public int?   SubTotal { get; init; }
    public int StepIndex  { get; init; } // 1..TotalSteps
    public int TotalSteps { get; init; } // z. B. 6
    
    // Overall (Maßnahmen)
    public int TotalItems     { get; init; }
    public int ProcessedItems { get; init; }

    // Worker
    public int WorkerIndex { get; init; }
    public int WorkerTotal { get; init; }
    public int WorkerDone  { get; init; }

    // Statistik
    public int MeasuresDone { get; init; }
    public int Regelungen   { get; init; }
    public int BvE          { get; init; }
    public int APS          { get; init; }
    public int IAV          { get; init; }
}