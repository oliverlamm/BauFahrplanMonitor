using BauFahrplanMonitor.Core.Configuration;
using Microsoft.Extensions.Configuration;
using NLog;

namespace BauFahrplanMonitor.Core.Services;

public class ConfigService {
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public AppConfig Raw        { get; }
    public AppConfig Effective  { get; }
    public string    SessionKey { get; }

    // ------------------------------------------------------------
    // ctor
    // ------------------------------------------------------------
    public ConfigService(string[]? args = null) {
        SessionKey = ResolveSessionKey(args);

        Logger.Info("=== ConfigService Initialisierung ===");
        Logger.Info($"SessionKey ermittelt: '{SessionKey}'");

        Raw = LoadRawConfig();

        LogAvailableSessions(Raw);

        Effective = Merge(Raw, SessionKey);

        Logger.Info("ConfigService Initialisierung abgeschlossen");
    }

    // ------------------------------------------------------------
    // Raw-Config laden
    // ------------------------------------------------------------
    private static AppConfig LoadRawConfig() {
        Logger.Info("Lade appsettings.json + appsettings.local.json");

        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json",       optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.local.json", optional: true,  reloadOnChange: true);

        var cfg = builder.Build();

        var result = new AppConfig();
        cfg.Bind(result);

        var rawSessions = cfg.GetSection("Sessions").GetChildren().ToList();
        Logger.Info($"[Config] Sessions im IConfiguration: {rawSessions.Count}");

        foreach (var s in rawSessions) {
            Logger.Info($"[Config] Session-Section gefunden: {s.Key}");
        }


        return result;
    }

    // ------------------------------------------------------------
    // SessionKey bestimmen
    // ------------------------------------------------------------
    private static string ResolveSessionKey(string[]? args) {
        if (args == null)
            return Environment.MachineName.Trim();
        for (var i = 0; i < args.Length - 1; i++) {
            if (args[i].Equals("--session", StringComparison.OrdinalIgnoreCase)) {
                return args[i + 1].Trim();
            }
        }

        return Environment.MachineName.Trim();
    }

    // ------------------------------------------------------------
    // Merge-Logik (Session anwenden)
    // ------------------------------------------------------------
    private static AppConfig Merge(AppConfig raw, string sessionKey) {
        // 🔥 FIX: case-insensitives Matching
        var session = raw.Sessions
            .FirstOrDefault(s =>
                string.Equals(s.Key, sessionKey, StringComparison.OrdinalIgnoreCase))
            .Value;

        if (session == null) {
            Logger.Warn(
                $"Keine Session-Konfiguration für '{sessionKey}' gefunden – verwende Default-Werte");
            return raw;
        }

        Logger.Info($"Session '{sessionKey}' erfolgreich geladen");

        return new AppConfig {
            Datenbank = Merge(raw.Datenbank, session.Datenbank),
            Allgemein = Merge(raw.Allgemein, session.Allgemein),
            Datei     = Merge(raw.Datei,     session.Datei),
            System    = raw.System,
            Sessions  = raw.Sessions
        };
    }

    // ------------------------------------------------------------
    // Merge: Allgemein
    // ------------------------------------------------------------
    private static AllgemeinConfig Merge(AllgemeinConfig d, AllgemeinConfig? o) {
        if (o == null)
            return d;

        var debugging = o.Debugging;

        var importThreads = debugging
            ? 1
            : (o.ImportThreads != 0 ? o.ImportThreads : d.ImportThreads);

        if (debugging && importThreads != 1) {
            Logger.Warn(
                "Debugging=true → ImportThreads wird zwangsweise auf 1 gesetzt (war {0})",
                o.ImportThreads);
        }

        return new AllgemeinConfig {
            Debugging          = debugging,
            ImportThreads      = importThreads,
            StopAfterException = o.StopAfterException
        };
    }

    // ------------------------------------------------------------
    // Merge: Datei
    // ------------------------------------------------------------
    private static DateiConfig Merge(DateiConfig d, DateiConfig? o)
        => o == null
            ? d
            : new DateiConfig {
                Archivieren        = o.Archivieren,
                NachImportLoeschen = o.NachImportLoeschen,
                Importpfad = string.IsNullOrWhiteSpace(o.Importpfad)
                    ? d.Importpfad
                    : o.Importpfad,
                Archivpfad = string.IsNullOrWhiteSpace(o.Archivpfad)
                    ? d.Archivpfad
                    : o.Archivpfad
            };

    // ------------------------------------------------------------
    // Merge: Datenbank
    // ------------------------------------------------------------
    private static DatenbankConfig Merge(DatenbankConfig d, DatenbankConfig? o)
        => o == null
            ? d
            : new DatenbankConfig {
                User                  = string.IsNullOrWhiteSpace(o.User) ? d.User : o.User,
                Password              = string.IsNullOrWhiteSpace(o.Password) ? d.Password : o.Password,
                Host                  = string.IsNullOrWhiteSpace(o.Host) ? d.Host : o.Host,
                Port                  = o.Port != 0 ? o.Port : d.Port,
                Database              = string.IsNullOrWhiteSpace(o.Database) ? d.Database : o.Database,
                EFLogging             = o.EFLogging,
                EFSensitiveLogging    = o.EFSensitiveLogging,
                ExpectedSchemaVersion = d.ExpectedSchemaVersion
            };

    // ------------------------------------------------------------
    // Logging-Helfer
    // ------------------------------------------------------------
    private static void LogAvailableSessions(AppConfig config) {
        if (config.Sessions.Count == 0) {
            Logger.Warn("Keine Sessions in der Konfiguration definiert");
            return;
        }

        Logger.Info("Verfügbare Sessions:");
        foreach (var key in config.Sessions.Keys) {
            Logger.Info($"  - {key}");
        }
    }

    public string BuildConnectionString() {
        var c = Effective.Datenbank;
        var cs =
            $"Host={c.Host};"                      +
            $"Port={c.Port};"                      +
            $"Database={c.Database};"              +
            $"Username={c.User};"                  +
            $"Password={c.Password};"              +
            $"Application Name=BauFahrplanMonitor;" + // 🔑 wichtig
            $"Maximum Pool Size=50;"                + // 🔑 wichtig
            $"Timeout=15;"                          +
            $"Command Timeout=120;"                 +
            $"Include Error Detail=true;";
        return cs;
    }
}