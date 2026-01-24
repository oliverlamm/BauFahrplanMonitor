namespace BauFahrplanMonitor.Api.Dto;

public sealed class BetriebsstelleGeoDto {
    public int     VzGNr { get; init; }
    public double  Lon   { get; init; }
    public double  Lat   { get; init; }
    public string? KmL   { get; init; }
    public double? KmI   { get; init; }
}
