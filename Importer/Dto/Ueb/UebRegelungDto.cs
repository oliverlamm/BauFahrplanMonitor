namespace BauFahrplanMonitor.Importer.Dto.Ueb;

public class UebRegelungDto {
    private string? _jsonRaw;
    public  string  Art         { get; set; } = "";
    public  string? AnchorRl100 { get; set; }
    public  string? BisRl100    { get; init; }

    public string? JsonRaw {
        get => _jsonRaw;
        init => _jsonRaw = string.IsNullOrWhiteSpace(value) ? null : value;
    }
}