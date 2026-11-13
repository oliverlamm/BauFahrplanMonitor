using BauFahrplanMonitor.Core.Importer.Dto.Shared;

namespace BauFahrplanMonitor.Core.Importer.Dto.Ueb;

public class UebDocumentDto : SharedDocumentDto {
    public DateOnly?              GueltigAb    { get; set; }
    public DateOnly?              GueltigBis   { get; set; }
    public new string             MasterRegion { get; set; } = "";
    public string                 Region       { get; set; } = "";
    public new List<SharedStreckeDto> Strecken     { get; set; } = [];
    public List<UebZugDto>        ZuegeRaw     { get; set; } = [];
    public List<UebZugDto>        Zuege        { get; set; } = [];
    public List<UebSevDto>        Sev          { get; set; } = [];
    public string?                StreckenJson { get; set; }
}