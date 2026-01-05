using System.Collections.Generic;

namespace BauFahrplanMonitor.Importer.Dto.Nfpl;

public sealed class NetzfahrplanZugDto {
    public long ZugNr { get; init; }

    public List<NetzfahrplanZugVarianteDto> Varianten { get; } = new();
}
