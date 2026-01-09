namespace BauFahrplanMonitor.Core.Importer.Dto.Fplo;

public class FploZugFahrplanDto {
    public long      LfdNr              { get; set; }
    public string    BstDs100           { get; set; } = "";
    public string    HalteArt           { get; set; } = "";
    public DateTime? AnkunftsZeit       { get; set; }
    public DateTime? AbfahrtsZeit       { get; set; }
    public string    Bemerkung          { get; set; } = "";
    public string    EbulaVglZug        { get; set; } = "";
    public int?      EbulaVglMbr        { get; set; }
    public string?   EbulaVglBrs        { get; set; }
    public int?      EbulaFahrplanHeft  { get; set; }
    public int?      EbulaFahrplanSeite { get; set; }
}