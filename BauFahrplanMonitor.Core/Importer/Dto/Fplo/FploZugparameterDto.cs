using System;

namespace BauFahrplanMonitor.Importer.Dto.Fplo;

public class FploZugparameterDto {
    public long?     ZugNr            { get; set; }
    public DateOnly? Verkehrstag      { get; set; }
    public string    WirktAbBstDs100  { get; set; } = "";
    public string    WirktBisBstDs100 { get; set; } = "";
    public string    Art              { get; set; } = "";
    public string    Wert             { get; set; } = "";
}