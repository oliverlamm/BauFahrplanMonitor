using System;
using System.Collections.Generic;

namespace BauFahrplanMonitor.Configuration;

public class AppConfig {
    public DatenbankConfig Datenbank { get; set; } = new();
    public AllgemeinConfig Allgemein { get; set; } = new();
    public DateiConfig     Datei     { get; set; } = new();
    public SystemConfig    System    { get; set; } = new();

    // 🔥 MUSS so initialisiert sein
    public Dictionary<string, SessionConfig> Sessions { get; set; }
        = new(StringComparer.OrdinalIgnoreCase);
}