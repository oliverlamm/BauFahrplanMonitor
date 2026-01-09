namespace BauFahrplanMonitor.Core.Importer.Dto.ZvF;

public class ZvFZugEntfallenDto {
    public long     Zugnr           { get; set; }
    public string   Zugbez          { get; set; } = "";
    public DateOnly Verkehrstag     { get; set; }
    public string   RegelungsartAlt { get; set; } = "";
}