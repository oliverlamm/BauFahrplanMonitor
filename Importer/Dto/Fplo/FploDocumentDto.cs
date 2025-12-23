using System;
using System.Collections.Generic;
using BauFahrplanMonitor.Importer.Dto.Shared;

namespace BauFahrplanMonitor.Importer.Dto.Fplo;

public class FploDocumentDto : SharedDocumentDto {
    public DateOnly?                    GueltigAb        { get; set; }
    public DateOnly?                    GueltigBis       { get; set; }
    public string                       MasterRegion     { get; set; } = "";
    public string                       Region           { get; set; } = "";
    public bool                         IstTeillieferung { get; set; }
    public bool                         IstNachtrag      { get; set; }
    public bool                         IstEntwurf       { get; set; }
    public List<SharedStreckeDto>       Strecken         { get; set; } = [];
    public List<FploZugDto>             Zuege            { get; set; } = [];
    public List<FploSevDto>             Sev              { get; set; } = [];
    public List<FploHaltausfallDto>     Haltausfall      { get; set; } = [];
    public List<FploZurueckgehaltenDto> Zurueckgehalten  { get; set; } = [];
    public List<FploZugparameterDto>    Zugparameter     { get; set; } = [];
    public List<FploRegelungDto>        Regelungen       { get; set; } = [];
    public string?                      StreckenJson     { get; set; }
}