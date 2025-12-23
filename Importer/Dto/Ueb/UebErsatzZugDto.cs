using System;
using BauFahrplanMonitor.Importer.Dto.Shared;

namespace BauFahrplanMonitor.Importer.Dto.Ueb;

public class UebErsatzZugDto :SharedZugDto{
    public int       Vespaetung    { get; set; }
    public long?     ImPlan        { get; set; }
    public DateTime? Abfahrt       { get; set; }
    public DateTime? Ankunft       { get; set; }
    public bool      NeuerFahrplan { get; set; } = false;
}