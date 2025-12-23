using System.Collections.Generic;
using BauFahrplanMonitor.Importer.Dto.Shared;

namespace BauFahrplanMonitor.Importer.Dto.ZvF;

public class ZvFZugDto : SharedZugDto {
    public string?                Aenderung    { get; set; }
    public bool                   Bedarf       { get; init; }
    public bool                   Sonder       { get; init; }
    public List<ZvFZugAbweichung> Abweichungen { get; set; } = [];
}