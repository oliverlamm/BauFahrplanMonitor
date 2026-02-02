namespace BauFahrplanMonitor.Core.Domain.Infrastruktur;

public sealed class Betriebsstelle
{
    public string  Ds100 { get; init; } = null!;
    public string  Name  { get; init; } = null!;
    public string? Plc   { get; init; }

    public double? Breite { get; init; }
    public double? Laenge { get; init; }

    public bool Elektrifiziert { get; init; }
    public bool IstBahnhof     { get; init; }
}
