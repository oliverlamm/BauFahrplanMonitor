using System;
using BauFahrplanMonitor.Importer.Dto.Shared;

namespace BauFahrplanMonitor.Importer.Dto.Fplo;

public class FploErsatzZugDto :SharedZugDto{
    public int       Vespaetung    { get; set; }
    public long?     ImPlan        { get; set; }
    public DateTime? Abfahrt       { get; set; }
    public DateTime? Ankunft       { get; set; }
    public bool      NeuerFahrplan { get; set; } = false;
}