namespace BauFahrplanMonitor.Importer.Helper;

public sealed class ImportProgressInfo {
    public string FileName { get; init; } = "";
    public string Step     { get; init; } = "";
    public int?   SubIndex { get; init; }
    public int?   SubTotal { get; init; }
    public int StepIndex  { get; init; } // 1..TotalSteps
    public int TotalSteps { get; init; } // z. B. 6
}