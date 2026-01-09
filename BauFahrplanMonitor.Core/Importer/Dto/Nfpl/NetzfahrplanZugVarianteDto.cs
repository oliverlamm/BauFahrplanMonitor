namespace BauFahrplanMonitor.Core.Importer.Dto.Nfpl;

public sealed class NetzfahrplanZugVarianteDto {
    public long?   TrainId     { get; init; }
    public string? TrainNumber { get; init; }

    public string? Kind        { get; init; }
    public string? TrainStatus { get; init; }
    public string? Remarks     { get; init; }
    public string? Region      { get; set; }

    public List<NetzfahrplanVerlaufDto> Verlauf { get; } = [];
}
