using System;
using BauFahrplanMonitor.Importer.Dto.Ueb;

namespace BauFahrplanMonitor.Importer.Dto.Fplo;

public class FploSevDto {
    public long            ZugNr            { get; set; }
    public DateOnly?       Verkehrstag      { get; set; }
    public string          StartDs100       { get; set; } = "";
    public string          EndDs100         { get; set; } = "";
    public string          Betreiber        { get; set; } = "";
    public string?         AusfallVonDs100  { get; set; } = "";
    public string?         AusfallBisDs100  { get; set; } = "";
    public long?           AusfallVonBstRef { get; set; }
    public long?           AusfallBisBstRef { get; set; }
    public int?            Verspaetung      { get; set; }
    public long?           ImPlan           { get; set; }
    public bool            NeuerFahrplan    { get; set; } = false;
    public UebErsatzZugDto Ersatzzug        { get; set; } = new();
}