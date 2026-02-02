namespace BauFahrplanMonitor.Core.Domain.Trassen;

public sealed class TrassenSucheParameter {
    public string   Von   { get; init; } = default!;
    public string   Nach  { get; init; } = default!;
    public DateTime Datum { get; init; }
}