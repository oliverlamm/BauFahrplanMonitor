namespace BauFahrplanMonitor.Configuration;

public class DateiConfig {
    public bool   Archivieren        { get; set; }
    public bool   NachImportLoeschen { get; set; }
    public string Importpfad         { get; set; } = string.Empty;
    public string Archivpfad         { get; set; } = string.Empty;
}