namespace BauFahrplanMonitor.Configuration;

public class DatenbankConfig {
    public string Host     { get; set; } = string.Empty;
    public int    Port     { get; set; }
    public string User     { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;

    public bool EFLogging             { get; set; }
    public bool EFSensitiveLogging    { get; set; }
    public int  ExpectedSchemaVersion { get; set; }
}