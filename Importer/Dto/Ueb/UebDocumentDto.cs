using System;
using System.Collections.Generic;
using BauFahrplanMonitor.Importer.Dto.Shared;

namespace BauFahrplanMonitor.Importer.Dto.Ueb;

public class UebDocumentDto : SharedDocumentDto {
    public DateOnly?              GueltigAb    { get; set; }
    public DateOnly?              GueltigBis   { get; set; }
    public string                 MasterRegion { get; set; } = "";
    public string                 Region       { get; set; } = "";
    public List<SharedStreckeDto> Strecken     { get; set; } = [];
    public List<UebZugDto>        Zuege        { get; set; } = [];
    public List<UebSevDto>        Sev          { get; set; } = [];
    public string?                StreckenJson { get; set; }
}