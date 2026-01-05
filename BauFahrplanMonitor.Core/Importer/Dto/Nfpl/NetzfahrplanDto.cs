using System.Collections.Generic;

namespace BauFahrplanMonitor.Importer.Dto.Nfpl;

public sealed class NetzfahrplanDto {
    public int                      FahrplanJahr { get; init; }
    public List<NetzfahrplanZugDto> Zuege        { get; } = [];
    public string?                   Region       { get; set; }
}