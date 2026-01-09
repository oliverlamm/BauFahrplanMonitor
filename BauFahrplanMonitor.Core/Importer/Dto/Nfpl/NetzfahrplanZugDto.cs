namespace BauFahrplanMonitor.Core.Importer.Dto.Nfpl;

public sealed class NetzfahrplanZugDto {
    public long ZugNr { get; init; }

    public List<NetzfahrplanZugVarianteDto> Varianten { get; } = new();
}
