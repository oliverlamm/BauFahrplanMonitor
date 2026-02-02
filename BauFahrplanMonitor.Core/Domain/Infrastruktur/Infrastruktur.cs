namespace BauFahrplanMonitor.Core.Domain.Infrastruktur;

public sealed class Infrastruktur {
    public string   Id           { get; init; } = null!;
    public string   Anzeigename  { get; init; } = null!;
    public int      Fahrplanjahr { get; init; }
    public DateOnly GueltigVon   { get; init; }
    public DateOnly GueltigBis   { get; init; }

    public IReadOnlyList<Betriebsstelle> Betriebsstellen { get; init; } = [];
    public IReadOnlyList<Triebfahrzeug>  Triebfahrzeuge  { get; init; } = [];
}