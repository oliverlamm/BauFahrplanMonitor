namespace BauFahrplanMonitor.Core.Importer.Dto.ZvF;

public class ZvFZugAbweichung {
    public long     Zugnummer    { get; init; }
    public DateOnly Verkehrstag  { get; init; }
    public string   Regelungsart { get; init; } = string.Empty;
    public string   JsonRaw      { get; init; } = string.Empty;
    public string?  AnchorRl100  { get; init; }
}