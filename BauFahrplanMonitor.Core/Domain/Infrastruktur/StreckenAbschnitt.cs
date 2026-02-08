namespace BauFahrplanMonitor.Core.Domain.Infrastruktur;

public class StreckenAbschnitt {
    public string VonRil100  { get; set; }
    public string BisRil100  { get; set; }
    public int    StreckenNr { get; set; }
    public double VonKm      { get; set; }
    public double BisKm      { get; set; }
}