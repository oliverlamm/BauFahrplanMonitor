using System;

namespace BauFahrplanMonitor.Importer.Dto.Fplo;

public class FploHaltausfallDto {
    public long?     ZugNr                  { get; set; }
    public DateOnly? Verkehrstag            { get; set; }
    public string?   ErsatzHaltDs100        { get; set; }
    public string?   AusfallenderHaltDs100  { get; set; }
}