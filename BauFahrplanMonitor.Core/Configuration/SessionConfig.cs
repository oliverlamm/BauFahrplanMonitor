namespace BauFahrplanMonitor.Core.Configuration;

public class SessionConfig {
    public DatenbankConfig Datenbank { get; set; } = new();
    public AllgemeinConfig Allgemein { get; set; } = new();
    public DateiConfig     Datei     { get; set; } = new();
}