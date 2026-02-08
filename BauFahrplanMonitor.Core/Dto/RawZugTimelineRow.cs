namespace BauFahrplanMonitor.Core.Dto;

public sealed class RawZugTimelineRow {

    public long    SeqNo { get; set; }
    public long    ZugNr { get; set; }
    public string? Kind  { get; set; }
    public string? Rl100 { get; set; }
    public string? Name  { get; set; }
    public string? Kbez  { get; set; }
    public string  Type  { get; set; } = null!;

    public TimeSpan PublishedArrival   { get; set; }
    public TimeSpan PublishedDeparture { get; set; }

    public int      DayOffset { get; set; }
    public DateTime SortDt    { get; set; }
}