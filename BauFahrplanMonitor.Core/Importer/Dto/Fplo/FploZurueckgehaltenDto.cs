using System;

namespace BauFahrplanMonitor.Importer.Dto.Fplo;

public class FploZurueckgehaltenDto {
    public long?     ZugNr            { get; set; }
    public DateOnly? Verkehrstag      { get; set; }
    public string    AbBstDs100       { get; set; } = "";
    public DateTime? ZurueckhaltenBis { get; set; }
}