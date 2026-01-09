namespace BauFahrplanMonitor.Core.Importer.Dto.BbpNeo;

public sealed class BbpNeoHeaderInfo {
    public int AnzMas { get; set; }

    public string? Ersteller  { get; set; }
    public string? BBPVersion { get; set; }

    public DateOnly? BplBeginn { get; set; }
    public DateOnly? BplEnde   { get; set; }
}