public sealed class BetriebsstellenListRowDto {
    public long   Id       { get; init; }
    public string Rl100    { get; init; } = "";
    public string Name     { get; init; } = "";
    public bool   IstBasis { get; init; }
}