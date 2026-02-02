namespace BauFahrplanMonitor.Core.Domain.Trassen;

public sealed class Trasse {
    public string   Von   { get; init; } = default!;
    public string   Nach  { get; init; } = default!;
    public DateTime Datum { get; init; }

    // später erweiterbar:
    // public TimeSpan Fahrzeit { get; init; }
    // public decimal Kosten { get; init; }
}