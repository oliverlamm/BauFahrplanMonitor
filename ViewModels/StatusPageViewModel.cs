using System;
using System.IO;
using System.Threading.Tasks;
using BauFahrplanMonitor.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using NLog;

public class StatusPageViewModel : ObservableObject {
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly ConfigService   _configService;
    private readonly DatabaseService _databaseService;

    private string _statusMessage;
    private string _statusColor;
    private string _statusBackgroundColor;
    private string _statusBorderColor;

    private string _dbUser;
    private string _dbHost;
    private string _dbPort;
    private string _dbName;
    private string _dbStatusIcon;
    private string _dbStatusIconColor;
    
    private string _confImportThreads;
    private bool   _confDebugging;
    private bool   _confStopAfterException;
    private bool   _confEFCoreLogging;
    private bool   _confEFCoreSensitiveLogging;
    private string _confSessionName;
    
    private string _dateiImportPfad;
    private string _dateiArchivPfad;
    private bool   _dateiIsArchivChecked;
    private bool   _dateiISDeleteAfterImportChecked;
    private string _dateiArchivePfadIcon;
    private string _dateiArchivePfadIconColor;
    private string _dateiImportPfadIcon;
    private string _dateiImportPfadIconColor;

    // Bindbare Eigenschaften
    public string StatusMessage {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string StatusColor {
        get => _statusColor;
        set => SetProperty(ref _statusColor, value);
    }

    public string StatusBackgroundColor {
        get => _statusBackgroundColor;
        set => SetProperty(ref _statusBackgroundColor, value);
    }

    public string StatusBorderColor {
        get => _statusBorderColor;
        set => SetProperty(ref _statusBorderColor, value);
    }

    public string DbUser {
        get => _dbUser;
        set => SetProperty(ref _dbUser, value);
    }

    public string DbHost {
        get => _dbHost;
        set => SetProperty(ref _dbHost, value);
    }

    public string DbPort {
        get => _dbPort;
        set => SetProperty(ref _dbPort, value);
    }

    public string DbName {
        get => _dbName;
        set => SetProperty(ref _dbName, value);
    }
    
    public string DbStatusIcon {
        get => _dbStatusIcon;
        set => SetProperty(ref _dbStatusIcon, value);
    }
    
    public string DbStatusIconColor {
        get => _dbStatusIconColor;
        set => SetProperty(ref _dbStatusIconColor, value);
    }

    public string ConfImportThreads {
        get => _confImportThreads;
        set => SetProperty(ref _confImportThreads, value);
    }

    public bool ConfDebugging {
        get => _confDebugging;
        set => SetProperty(ref _confDebugging, value);
    }

    public bool ConfStopAfterException {
        get => _confStopAfterException;
        set => SetProperty(ref _confStopAfterException, value);
    }

    public bool ConfEFCoreLogging {
        get => _confEFCoreLogging;
        set => SetProperty(ref _confEFCoreLogging, value);
    }

    public bool ConfEFCoreSensitiveLogging {
        get => _confEFCoreSensitiveLogging;
        set => SetProperty(ref _confEFCoreSensitiveLogging, value);
    }
    
    public string ConfSessionName {
        get => _confSessionName;
        set => SetProperty(ref _confSessionName, value);
    }

    public string DateiImportpfad {
        get => _dateiImportPfad;
        set => SetProperty(ref _dateiImportPfad, value);
    }

    public string DateiImportpfadIcon {
        get => _dateiImportPfadIcon;
        set => SetProperty(ref _dateiImportPfadIcon, value);
    }

    public string DateiImportpfadIconColor {
        get => _dateiImportPfadIconColor;
        set => SetProperty(ref _dateiImportPfadIconColor, value);
    }

    public string DateiArchivePfadIcon {
        get => _dateiArchivePfadIcon;
        set => SetProperty(ref _dateiArchivePfadIcon, value);
    }

    public string DateiArchivePfadIconColor {
        get => _dateiArchivePfadIconColor;
        set => SetProperty(ref _dateiArchivePfadIconColor, value);
    }

    public string DateiArchivpfad {
        get => _dateiArchivPfad;
        set => SetProperty(ref _dateiArchivPfad, value);
    }

    public bool DateiIsArchiveChecked {
        get => _dateiIsArchivChecked;
        set => SetProperty(ref _dateiIsArchivChecked, value);
    }

    public bool DateiIsDeleteAfterImportChecked {
        get => _dateiISDeleteAfterImportChecked;
        set => SetProperty(ref _dateiISDeleteAfterImportChecked, value);
    }

    public StatusPageViewModel(ConfigService configService, DatabaseService databaseService) {
        _configService   = configService;
        _databaseService = databaseService;

        var sessionKey = _configService.SessionKey;

        Logger.Info($"StatusPageViewModel initialisiert für Session: {sessionKey}");

        ConfSessionName = Environment.MachineName;
        
        // Asynchrone Methode aufrufen
        _ = GetConfigurationAsync();
        _ = LoadAsync();
    }

    private async Task LoadAsync() {
        await GetConfigurationAsync();

        // DB-Check explizit in Background-Thread
        _ = Task.Run(CheckDatabaseStatusAsync);
    }


    private async Task GetConfigurationAsync() {
        ConfImportThreads          = _configService.Effective.Allgemein.ImportThreads.ToString();
        ConfDebugging              = _configService.Effective.Allgemein.Debugging;
        ConfStopAfterException     = _configService.Effective.Allgemein.StopAfterException;
        ConfEFCoreLogging          = _configService.Effective.Datenbank.EFLogging;
        ConfEFCoreSensitiveLogging = _configService.Effective.Datenbank.EFSensitiveLogging;
        DateiImportpfad            = _configService.Effective.Datei.Importpfad.ToString();

        DateiIsArchiveChecked           = _configService.Effective.Datei.Archivieren;
        DateiIsDeleteAfterImportChecked = _configService.Effective.Datei.NachImportLoeschen;


        // DateiArchivePfad
        DateiArchivpfad = _configService.Effective.Datei.Archivpfad;
        if (Directory.Exists(_configService.Effective.Datei.Archivpfad)) {
            if (CanWriteToDirectory(_configService.Effective.Datei.Archivpfad)) {
                DateiArchivePfadIconColor = "#0BCB58"; // grün
                DateiArchivePfadIcon      = @"";      // Haken
                Logger.Info($"Archivpfad {_configService.Effective.Datei.Archivpfad} existiert und ist beschreibbar");
            }
            else {
                DateiArchivePfadIconColor = "#FFB90F"; // gelb
                DateiArchivePfadIcon      = @"";      // Warn-Dreieck
                Logger.Warn(
                    $"Archivpfad {_configService.Effective.Datei.Archivpfad} existiert, ist aber NICHT beschreibbar");
            }
        }
        else {
            DateiArchivePfadIconColor = "#E5533D"; // rot
            DateiArchivePfadIcon      = @"";      // Kreuz
            Logger.Warn($"Archivpfad {_configService.Effective.Datei.Archivpfad} existiert nicht");
        }


        // DateiImportpfad
        DateiImportpfad = _configService.Effective.Datei.Importpfad;
        if (Directory.Exists(_configService.Effective.Datei.Importpfad)) {
            DateiImportpfadIconColor = "#0BCB58";
            DateiImportpfadIcon      = @"";
            Logger.Info($"ImportPfad {_configService.Effective.Datei.Importpfad} existiert");
        }
        else {
            DateiImportpfadIconColor = "#FF0000";
            DateiImportpfadIcon      = @"";
            Logger.Info($"ImportPfad {_configService.Effective.Datei.Importpfad} fehlt");
        }
    }

    private static bool CanWriteToDirectory(string path) {
        try {
            var testFile = Path.Combine(
                path,
                $".write_test_{Guid.NewGuid():N}.tmp"
            );

            using (File.Create(testFile, 1, FileOptions.DeleteOnClose)) {
            }

            return true;
        }
        catch {
            return false;
        }
    }


    // Datenbankstatus prüfen
    private async Task CheckDatabaseStatusAsync() {
        try {
            var databaseCheckResult =
                await _databaseService.CheckDatabaseAsync(_configService.Effective.Datenbank.ExpectedSchemaVersion);

            Logger.Info($"DatenbankStatus: {databaseCheckResult.Status}");

            if (databaseCheckResult.Status == DatabaseService.DatabaseHealthStatus.Error) {
                DbStatusIcon      = @"";
                DbStatusIconColor = "#E5533D";
                return;
            }

            switch (databaseCheckResult.Status) {
                case DatabaseService.DatabaseHealthStatus.Ok:
                    DbStatusIcon      = @"";
                    DbStatusIconColor = "#0BCB58";
                    StatusMessage     = $"Schema OK: {databaseCheckResult.CurrentSchemaVersion}";
                    break;

                case DatabaseService.DatabaseHealthStatus.Warning:
                    DbStatusIcon      = @"";
                    DbStatusIconColor = "#FFB90F";
                    StatusMessage =
                        $"Schema: {databaseCheckResult.CurrentSchemaVersion}, erwartet: {_configService.Effective.Datenbank.ExpectedSchemaVersion}";
                    break;
            }

            // Verbindungsinformationen setzen
            DbUser = _configService.Effective.Datenbank.User;
            DbHost = _configService.Effective.Datenbank.Host;
            DbPort = _configService.Effective.Datenbank.Port.ToString();
            DbName = _configService.Effective.Datenbank.Database;
        }
        catch (Exception ex) {
            StatusColor       = "#FFDDDD"; // Hellroter Hintergrund
            StatusBorderColor = "#FF0000"; // Roter Rahmen
            StatusMessage     = $"Fehler: {ex.Message}";
            Logger.Error(ex, "Fehler bei der Datenbankabfrage.");
        }
    }
}