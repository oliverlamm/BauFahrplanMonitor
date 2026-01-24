namespace BauFahrplanMonitor.Api.Dto;

public sealed class BetriebsstelleUpdateDto {
    public string Name    { get; set; } = "";
    public string Zustand { get; set; } = "";

    public int TypId        { get; set; }
    public int RegionId     { get; set; }
    public int NetzbezirkId { get; set; }

    public bool IstBasis { get; set; }
}