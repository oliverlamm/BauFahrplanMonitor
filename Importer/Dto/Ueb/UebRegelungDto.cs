namespace BauFahrplanMonitor.Importer.Dto.Ueb;

public class UebRegelungDto {
    public string  Art         { get; set; } = "";
    public string? AnchorRl100 { get; set; }
    public string  JsonRaw     { get; set; } = "";
}