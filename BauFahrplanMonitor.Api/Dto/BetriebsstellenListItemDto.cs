namespace BauFahrplanMonitor.Api.Dto;

public sealed class BetriebsstellenListItemDto {
    public long   Id       { get; init; }
    public string Rl100    { get; init; } = "";
    public string Name     { get; init; } = "";
    public bool   IstBasis { get; init; }

    // fertig für UI
    public string Label => $"{Name} [{Rl100}]";
}