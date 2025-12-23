namespace BauFahrplanMonitor.Importer.Helper;

public sealed class UpsertProgressInfo {
    public required UpsertPhase Phase   { get; init; } // Zuege | Entfallen
    public required int         Current { get; init; } // innerhalb der Phase
    public required int         Total   { get; init; } // innerhalb der Phase
}

public enum UpsertPhase {
    Zuege,
    Entfallen
}