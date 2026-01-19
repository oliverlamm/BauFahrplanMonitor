namespace BauFahrplanMonitor.Core.Importer.Helper;

public enum ImportFileOutcome {
    Success,
    Failed,
    Skipped   // z.B. ungültiges XML
}
