using BauFahrplanMonitor.Core.Importer.Dto.Shared;

namespace BauFahrplanMonitor.Core.Importer.Dto.ZvF;

public class ZvFDocumentDto : SharedDocumentDto {
    public DateOnly?                BauDatumVon  { get; set; }
    public DateOnly?                BauDatumBis  { get; set; }
    public DateOnly?                AntwortBis   { get; set; }
    public bool                     Endstueck    { get; set; }
    public List<SharedStreckeDto>   Strecken     { get; set; } = [];
    public List<ZvFZugDto>          ZuegeRaw     { get; set; } = [];
    public List<ZvFZugDto>          Zuege        { get; set; } = [];
    public List<ZvFZugEntfallenDto> Entfallen    { get; set; } = [];
    public string?                  StreckenJson { get; set; }
    
}