using BauFahrplanMonitor.Core.Importer.Dto.Shared;

namespace BauFahrplanMonitor.Core.Importer.Dto.Ueb;

public class UebZugDto : SharedZugDto {
    public bool                  Bedarf                 { get; set; } = false;
    public decimal?              ZugGattung             { get; set; }
    public bool                  IstSicherheitsrelevant { get; set; }
    public bool                  LauterZug              { get; set; }
    public long?                 Vmax                   { get; set; }
    public string?               Tfzf                   { get; set; } = "";
    public long?                 Last                   { get; set; }
    public long?                 Laenge                 { get; set; }
    public string?               Bremssystem            { get; set; } = "";
    public bool                  Ebula                  { get; set; }
    public bool                  IstAusfall             { get; set; }
    public string?               FploAbschnitt          { get; set; } = "";
    public bool                  IstVorplan             { get; set; }
    public List<UebKnotenDto>    Knotenzeiten           { get; set; } = [];
    public List<UebRegelungDto>? Regelungen             { get; set; } = [];
}