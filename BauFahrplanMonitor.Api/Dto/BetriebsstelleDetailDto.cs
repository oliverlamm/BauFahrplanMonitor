namespace BauFahrplanMonitor.Api.Dto;

public sealed class BetriebsstelleDetailDto {
    public long   Id      { get; set; }
    public string Rl100   { get; set; } = "";
    public string Name    { get; set; } = "";
    public string Zustand { get; set; } = "";

    public long   TypId { get; set; }
    public string Typ   { get; set; } = "";

    public long   RegionId { get; set; }
    public string Region   { get; set; } = "";

    public long   NetzbezirkId { get; set; }
    public string Netzbezirk   { get; set; } = "";

    public bool IstBasis { get; set; }

    public IReadOnlyList<BetriebsstelleGeoDto> Geo { get; set; } = [];
}