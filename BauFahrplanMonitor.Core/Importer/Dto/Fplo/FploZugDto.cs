using BauFahrplanMonitor.Core.Importer.Dto.Shared;

namespace BauFahrplanMonitor.Core.Importer.Dto.Fplo;

public class FploZugDto : SharedZugDto {
    public bool                     Bedarf                 { get; set; } = false;
    public decimal?                 ZugGattung             { get; set; }
    public bool                     IstSicherheitsrelevant { get; set; } = false;
    public bool                     LauterZug              { get; set; } = false;
    public long?                    Vmax                   { get; set; }
    public string?                  Tfzf                   { get; set; } = "";
    public long?                    Last                   { get; set; }
    public long?                    Laenge                 { get; set; }
    public string?                  Bremssystem            { get; set; } = "";
    public bool                     Ebula                  { get; set; } = false;
    public bool                     IstAusfall             { get; set; } = false;
    public string?                  FploAbschnitt          { get; set; } = "";
    public bool                     IstVorplan             { get; set; } = false;
    public List<FploZugFahrplanDto> Fahrplan               { get; set; } = [];
    public bool                     IstErsatzzug            { get; set; } = false;
}