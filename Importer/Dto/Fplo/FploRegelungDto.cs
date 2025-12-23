namespace BauFahrplanMonitor.Importer.Dto.Fplo;

public class FploRegelungDto {
    public string  Art         { get; set; } = "";
    public string? AnchorRl100 { get; set; }
    public string  JsonRaw     { get; set; } = "";
}