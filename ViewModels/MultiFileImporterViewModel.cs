using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Avalonia.Media;
using Avalonia.Threading;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Helpers;
using BauFahrplanMonitor.Importer.Helper;
using BauFahrplanMonitor.Importer.Interface;
using BauFahrplanMonitor.Resolver;
using BauFahrplanMonitor.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace BauFahrplanMonitor.ViewModels;

public partial class MultiFileImporterViewModel : ObservableObject {
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly ConfigService                     _configService;
    private readonly StatusMessageService              _statusMessages;
    private readonly DatabaseService                   _databaseService;
    private readonly IFileImporterFactory              _importerFactory;
    private readonly ImporterTyp                       _importerTyp;
    private readonly SharedReferenceResolver           _resolver;
    private readonly IDbContextFactory<UjBauDbContext> _dbFactory;


    private Dictionary<string, ImportDbInfo>? _dbImportCache;

    // =====================================================
    // Header
    // =====================================================
    public string Title { get; }

    private string         _statusColor = "#228B22";
    private ImporterStatus _status      = ImporterStatus.Bereit;

    private ImporterStatus Status {
        get => _status;
        set {
            if (!SetProperty(ref _status, value)) return;
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusBrush));
        }
    }

    public IBrush StatusBrush =>
        Status switch {
            ImporterStatus.Bereit        => Brushes.Gray,
            ImporterStatus.Scannen       => Brushes.RoyalBlue,
            ImporterStatus.Importieren   => Brushes.DodgerBlue,
            ImporterStatus.Abgeschlossen => Brushes.ForestGreen,
            ImporterStatus.Abbruch       => Brushes.DarkOrange,
            ImporterStatus.Fehler        => Brushes.IndianRed,
            _                            => Brushes.Gray
        };

    public string StatusText =>
        Status switch {
            ImporterStatus.Bereit        => "Bereit",
            ImporterStatus.Scannen       => "Scanne…",
            ImporterStatus.Importieren   => "Importiere…",
            ImporterStatus.Abgeschlossen => "Abgeschlossen",
            ImporterStatus.Abbruch       => "Abbruch",
            ImporterStatus.Fehler        => "Fehler",
            _                            => "Unbekannt"
        };

    public string StatusColor {
        get => _statusColor;
        private set => SetProperty(ref _statusColor, value);
    }

    // =====================================================
    // Importer-Typ
    // =====================================================
    public ImporterTyp ImporterTyp { get; }
    public bool        ShowFilters => ImporterTyp == ImporterTyp.ZvFExport;

    // =====================================================
    // Importpfad
    // =====================================================
    private string _importDirectory = string.Empty;

    public string ImportDirectory {
        get => _importDirectory;
        private set {
            SetProperty(ref _importDirectory, value);
            OnPropertyChanged(nameof(ImportDirectoryExists));
            OnPropertyChanged(nameof(CanScan));
        }
    }

    public bool ImportDirectoryExists =>
        Directory.Exists(ImportDirectory);

    // =====================================================
    // Threads
    // =====================================================
    private int _threadCount;

    public int ThreadCount {
        get => _threadCount;
        private set => SetProperty(ref _threadCount, value);
    }

    public ObservableCollection<ImportThreadViewModel> Threads { get; }

    // =====================================================
    // Status / Buttons
    // =====================================================
    private bool _isImportRunning;

    public bool IsImportRunning {
        get => _isImportRunning;
        private set {
            SetProperty(ref _isImportRunning, value);
            OnPropertyChanged(nameof(CanStart));
            OnPropertyChanged(nameof(CanStop));
            OnPropertyChanged(nameof(CanScan));
        }
    }

    private bool _isScanning;

    public bool IsScanning {
        get => _isScanning;
        private set {
            SetProperty(ref _isScanning, value);
            OnPropertyChanged(nameof(CanScan));
            OnPropertyChanged(nameof(CanStop));
        }
    }

    public bool CanScan =>
        ImportDirectoryExists && !IsScanning && !IsImportRunning;

    // 🔴 ENTSCHEIDEND: Start hängt NUR an Queue
    public bool CanStart =>
        QueueCount > 0 && !IsImportRunning && !IsScanning;

    public bool CanStop =>
        IsScanning || IsImportRunning;

    // =====================================================
    // Fortschritt
    // =====================================================
    private int _totalFiles;

    public int TotalFiles {
        get => _totalFiles;
        private set {
            SetProperty(ref _totalFiles, value);
            OnPropertyChanged(nameof(OverallProgress));
            OnPropertyChanged(nameof(OverallProgressText));
        }
    }

    private int _processedFiles;

    public int ProcessedFiles {
        get => _processedFiles;
        private set {
            SetProperty(ref _processedFiles, value);
            OnPropertyChanged(nameof(OverallProgress));
            OnPropertyChanged(nameof(OverallProgressText));
        }
    }

    public double OverallProgress =>
        TotalFiles == 0 ? 0 : (double)ProcessedFiles / TotalFiles;

    public string OverallProgressText =>
        $"{ProcessedFiles} / {TotalFiles}";


    // =====================================================
    // Filter
    // =====================================================
    public bool FilterAll  { get; set; } = true;
    public bool FilterZvF  { get; set; }
    public bool FilterÜB   { get; set; }
    public bool FilterFplo { get; set; }

    // =====================================================
    // Queue & Worker
    // =====================================================
    private readonly ConcurrentQueue<ImportFileItem> _importQueue = new();
    private readonly List<Task>                      _workerTasks = new();

    public int QueueCount => _importQueue.Count;

    private CancellationTokenSource? _scanCts;
    private CancellationTokenSource? _importCts;

    // =====================================================
    // ctor
    // =====================================================
    public MultiFileImporterViewModel(
        ConfigService                     configService,
        StatusMessageService              statusMessages,
        DatabaseService                   databaseService,
        IDbContextFactory<UjBauDbContext> dbFactory,
        SharedReferenceResolver           resolver,
        string                            title,
        IFileImporterFactory              importerFactory,
        ImporterTyp                       importerTyp)
    {
        _configService   = configService;
        _statusMessages  = statusMessages;
        _databaseService = databaseService;
        _dbFactory       = dbFactory; // ✅
        _resolver        = resolver;  // ✅
        _importerFactory = importerFactory;
        _importerTyp     = importerTyp;
        ImporterTyp      = importerTyp;
        Title            = title;

        Threads = [];

        InitializeFromConfig();
    }

    // =====================================================
    // Initialisierung
    // =====================================================
    private void InitializeFromConfig() {
        ImportDirectory = _configService.Effective.Datei.Importpfad;
        CreateThreadCards(ResolveThreadCount());

        if (ImportDirectoryExists) return;
        Status = ImporterStatus.Fehler;
        _statusMessages.Error(
            $"Importverzeichnis existiert nicht: {ImportDirectory}");
    }

    private int ResolveThreadCount() {
        if (!_configService.Effective.Allgemein.Debugging) return _configService.Effective.Allgemein.ImportThreads;
        Logger.Info("Debugging aktiv → Thread-Anzahl auf 1 begrenzt");
        return 1;
    }

    private void CreateThreadCards(int count) {
        Threads.Clear();
        ThreadCount = count;

        for (var i = 1; i <= count; i++)
            Threads.Add(new ImportThreadViewModel(i));
    }

    // =====================================================
    // Commands
    // =====================================================
    [RelayCommand]
    private async Task ScanAsync() {
        if (!ImportDirectoryExists)
            return;

        Status         = ImporterStatus.Scannen;
        IsScanning     = true;
        IsScanning     = true;
        TotalFiles     = 0;
        ProcessedFiles = 0;

        _importQueue.Clear();
        OnPropertyChanged(nameof(QueueCount));
        OnPropertyChanged(nameof(CanStart));

        _scanCts = new CancellationTokenSource();

        try {
            await Task.Run(() =>
                    BuildQueueWithStatusAsync(_scanCts.Token),
                _scanCts.Token);

            TotalFiles = _importQueue.Count;

            Status = ImporterStatus.Abgeschlossen;
            _statusMessages.Success(
                $"Scan abgeschlossen ({QueueCount} importierbare Dateien)");
        }
        catch (OperationCanceledException) {
            Status = ImporterStatus.Abbruch;
        }
        catch (Exception ex) {
            Status = ImporterStatus.Fehler;
            _statusMessages.Error("Fehler beim Scannen des Importverzeichnisses");
            Logger.Error(ex);
        }
        finally {
            IsScanning = false;
            _scanCts   = null;

            OnPropertyChanged(nameof(QueueCount));
            OnPropertyChanged(nameof(CanStart));
        }
    }

    [RelayCommand]
    private async Task StartAsync() {
        if (_importQueue.IsEmpty)
            return;

        Status          = ImporterStatus.Importieren;
        IsImportRunning = true;

        _importCts = new CancellationTokenSource();
        _workerTasks.Clear();

        ProcessedFiles = 0;

        // -------------------------------------------------
        // 5.3 Region-Cache Warm-Up (EINMAL, VOR Threads)
        // -------------------------------------------------
        try {
            await using (var db = await _dbFactory.CreateDbContextAsync(_importCts.Token)) {
                await _resolver.WarmUpRegionCacheAsync(db, _importCts.Token);
                
                Logger.Info(
                    "[Region.Cache] Ready: Size={0}, Stats={1}",
                    _resolver.RegionCacheSize,
                    _resolver.GetRegionStats());
            }
        }
        catch (OperationCanceledException) {
            Status          = ImporterStatus.Abbruch;
            IsImportRunning = false;
            return;
        }
        catch (Exception ex) {
            Logger.Error(ex, "Fehler beim Region-Cache Warm-Up");
            Status          = ImporterStatus.Fehler;
            IsImportRunning = false;
            return;
        }

        // ------c-------------------------------------------
        // JETZT Worker starten
        // -------------------------------------------------
        foreach (var t in Threads) {
            t.Status = ImportThreadStatus.Bereit;
        }

        foreach (var t in Threads) {
            _workerTasks.Add(Task.Run(
                () => WorkerLoopAsync(t, _importCts.Token),
                _importCts.Token));
        }

        try {
            await Task.WhenAll(_workerTasks);
            Status = ImporterStatus.Abgeschlossen;
        }
        catch (OperationCanceledException) {
            Status = ImporterStatus.Abbruch;
        }
        finally {
            IsImportRunning = false;
            _importCts      = null;

            OnPropertyChanged(nameof(QueueCount));
            OnPropertyChanged(nameof(CanStart));
        }
    }

    [RelayCommand]
    private void Stop() {
        if (IsScanning) {
            _scanCts?.Cancel();
            Status = ImporterStatus.Abbruch;
            return;
        }

        if (!IsImportRunning) return;
        _importCts?.Cancel();
        Status = ImporterStatus.Abbruch;
    }

    // =====================================================
    // Scan + Queue-Aufbau
    // =====================================================
    private async Task BuildQueueWithStatusAsync(CancellationToken token) {
        var files = ImporterTyp switch {
            ImporterTyp.Direkt    => ScanFilesDirect(),
            ImporterTyp.ZvFExport => ScanFilesZvFExport(),
            _                     => []
        };

        LogDebugJson(
            "Scan-Ergebnis (Dateiliste)",
            files.Select(f => new { File = f }).ToList());

        Dispatcher.UIThread.Invoke(() => {
            TotalFiles     = files.Count;
            ProcessedFiles = 0;
        });

        await BuildImportCacheAsync();

        // 1️⃣ temporäre Liste für sortierbare Items
        var items = new List<ImportFileItem>();

        var index = 0;

        foreach (var file in files) {
            token.ThrowIfCancellationRequested();
            index++;

            Dispatcher.UIThread.Invoke(() => {
                ProcessedFiles = index;
                Status         = ImporterStatus.Scannen;
                IsScanning     = true;
            });

            var item = TryCreateImportItem(file);
            if (item != null)
                items.Add(item);
        }

        // 2️⃣ SORTIERUNG: älteste zuerst
        items = items
            .OrderBy(i => i.SortTimestamp)
            .ToList();

        // 3️⃣ Queue leeren (Sicherheit)
        _importQueue.Clear();

        // 4️⃣ SORTIERT enqueuen
        foreach (var item in items) {
            _importQueue.Enqueue(item);
        }

        // 5️⃣ UI informieren
        Dispatcher.UIThread.Invoke(() => {
            OnPropertyChanged(nameof(QueueCount));
            OnPropertyChanged(nameof(CanStart));
        });


        LogDebugJson(
            "Queue-Inhalt nach Build",
            _importQueue.Select(i => new {
                i.FilePath,
                i.SortTimestamp,
                i.FileType
            }).ToList());
    }

    // =====================================================
    // Worker
    // =====================================================
    private async Task WorkerLoopAsync(
        ImportThreadViewModel threadVm,
        CancellationToken     token) {
        try {
            while (!token.IsCancellationRequested &&
                   _importQueue.TryDequeue(out var item)) {
                var totalZuege     = 0;
                var totalEntfallen = 0;
                var doneZuege      = 0;
                var doneEntfallen  = 0;

                var progress = new Progress<ImportProgressInfo>(info => {
                    Dispatcher.UIThread.Post(() => {
                        threadVm.FileName = info.FileName;
                        threadVm.Status   = ImportThreadStatus.Importieren;

                        // -----------------------------
                        // StatusMessage (Detail)
                        // -----------------------------
                        threadVm.StatusMessage = info.Step;

                        // -----------------------------
                        // ThreadProgressText = Steps
                        // -----------------------------
                        threadVm.ThreadProgressText =
                            $"{info.StepIndex} / {info.TotalSteps}";

                        // -----------------------------
                        // ProgressBar = ZÜGE + ENTFALLEN
                        // -----------------------------
                        if (!info.SubIndex.HasValue || !info.SubTotal.HasValue) return;
                        // Initiale Phase erkennen
                        if (info.Step.StartsWith("Upsert Züge")) {
                            totalZuege = info.SubTotal.Value;
                            doneZuege  = info.SubIndex.Value;
                        }
                        else if (info.Step.StartsWith("Upsert Entfallen")) {
                            totalEntfallen = info.SubTotal.Value;
                            doneEntfallen  = info.SubIndex.Value;
                        }

                        var total = totalZuege + totalEntfallen;
                        var done  = doneZuege  + doneEntfallen;

                        if (total > 0) {
                            threadVm.ThreadProgress =
                                (int)(done * 100.0 / total);
                        }
                    });
                });

                await using var db = await _dbFactory.CreateDbContextAsync(token);
                var importer = _importerFactory.GetImporter(_importerTyp);
                await importer.ImportAsync(db, item, progress, token);
                db.ChangeTracker.Clear();

                // Thread: Reset für nächste Datei
                await Dispatcher.UIThread.InvokeAsync(() => {
                    threadVm.ThreadProgress     = 0;
                    threadVm.ThreadProgressText = string.Empty;
                });

                // Coordinator: Overall
                await Dispatcher.UIThread.InvokeAsync(() => {
                    ProcessedFiles++;
                    OnPropertyChanged(nameof(OverallProgress));
                    OnPropertyChanged(nameof(OverallProgressText));
                });
            }

            await Dispatcher.UIThread.InvokeAsync(() => {
                threadVm.Status        = ImportThreadStatus.Beendet;
                threadVm.FileName      = string.Empty;
                threadVm.StatusMessage = "Keine Dateien mehr";
            });
        }
        catch (OperationCanceledException) {
            Dispatcher.UIThread.Post(() => {
                threadVm.Status        = ImportThreadStatus.Abbruch;
                threadVm.StatusMessage = "Abgebrochen";
            });
        }
        catch (Exception ex) {
            Dispatcher.UIThread.Post(() => {
                threadVm.Status        = ImportThreadStatus.Fehler;
                threadVm.StatusMessage = ex.Message;
            });

            Logger.Error(ex, "Fehler im Import-Worker");
        }
    }

    // =====================================================
    // Hilfsmethoden (Scan / Filter / DB)
    // =====================================================
    private List<string> ScanFilesDirect()
        => Directory.Exists(ImportDirectory)
            ? Directory.EnumerateFiles(ImportDirectory, "*.*", SearchOption.AllDirectories).ToList()
            : [];

    private List<string> ScanFilesZvFExport() {
        if (!Directory.Exists(ImportDirectory))
            return [];

        var files = new HashSet<string>();
        foreach (var pattern in GetZvFSearchPatterns())
        foreach (var f in Directory.EnumerateFiles(
                     ImportDirectory, pattern, SearchOption.AllDirectories))
            files.Add(f);

        return files.ToList();
    }

    private ImportFileItem? TryCreateImportItem(string file) {
        if (ImporterTyp == ImporterTyp.ZvFExport && !ShouldImport(file))
            return null;

        var exportTimestamp = ReadTimestampFromHeader(file);
        var fileType        = ResolveFileType(file);

        Logger.Debug($"[QUEUE] {Path.GetFileName(file)} → {fileType}");

        return new ImportFileItem(file, exportTimestamp, fileType);
    }

    private static ImportMode ResolveFileType(string file) {
        var name = Path.GetFileName(file);
        if (string.IsNullOrWhiteSpace(name))
            return ImportMode.None;

        if (name.StartsWith("ZvF", StringComparison.OrdinalIgnoreCase)) return ImportMode.ZvF;
        if (name.StartsWith("ÜB",  StringComparison.OrdinalIgnoreCase)) return ImportMode.UeB;
        return name.StartsWith("Fplo", StringComparison.OrdinalIgnoreCase) ? ImportMode.Fplo : ImportMode.None;
    }

    private IEnumerable<string> GetZvFSearchPatterns() {
        if (FilterAll)
            return new[] { "ZvF*.xml", "ÜB*.xml", "Fplo*.xml" };

        var list = new List<string>();
        if (FilterZvF) list.Add("ZvF*.xml");
        if (FilterÜB) list.Add("ÜB*.xml");
        if (FilterFplo) list.Add("Fplo*.xml");
        return list;
    }

    private bool ShouldImport(string filePath) {
        if (_dbImportCache == null) {
            Logger.Debug($"[IMPORT] Kein DB-Cache → importiere {Path.GetFileName(filePath)}");
            return true;
        }

        var fileName = Path.GetFileName(filePath);

        if (!_dbImportCache.TryGetValue(fileName, out var dbInfo)) {
            Logger.Debug($"[IMPORT] Kein DB-Eintrag → importiere {fileName}");
            return true;
        }

        if (dbInfo.ImportTimestamp == null) {
            Logger.Debug($"[IMPORT] ImportTimestamp NULL → importiere {fileName}");
            return true;
        }

        Logger.Debug(
            $"[SKIP] {fileName} bereits importiert am {dbInfo.ImportTimestamp:yyyy-MM-dd HH:mm:ss}");
        CleanupFile((filePath));

        return false;
    }

    private void CleanupFile(string filePath) {
        if (_configService.Effective.Datei is { Archivieren: false, NachImportLoeschen: false })
            return;

        if (_configService.Effective.Datei.Archivieren) {
            var target = Path.Combine(
                _configService.Effective.Datei.Archivpfad,
                Path.GetFileName(filePath));

            Directory.CreateDirectory(_configService.Effective.Datei.Archivpfad);
            File.Copy(filePath, target, overwrite: true);
        }

        if (_configService.Effective.Datei.NachImportLoeschen) {
            File.Delete(filePath);
        }
    }

    private static DateTime ReadTimestampFromHeader(string filePath) {
        try {
            using var stream = File.OpenRead(filePath);
            var       doc    = XDocument.Load(stream);

            var ts = doc.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "Timestamp");

            if (ts != null &&
                DateTime.TryParse(ts.Value, out var parsed))
                return parsed;
        }
        catch {
            // ignore
        }

        return File.GetCreationTimeUtc(filePath);
    }

    private Task BuildImportCacheAsync() {
        Logger.Info("Lade Import-Cache aus der Datenbank…");

        var cache = new Dictionary<string, ImportDbInfo>(
            StringComparer.OrdinalIgnoreCase);

        using var db = _databaseService.CreateNewContext();

        foreach (var d in GetZvfDokumenteAsync(db))
            if (d.FileName != null)
                cache[d.FileName] = d;

        foreach (var d in GetUebDokumenteAsync(db))
            if (d.FileName != null)
                cache[d.FileName] = d;

        foreach (var d in GetFploDokumenteAsync(db))
            if (d.FileName != null)
                cache[d.FileName] = d;

        _dbImportCache = cache;

        Logger.Info($"Import-Cache geladen ({cache.Count} Einträge)");
        return Task.CompletedTask;
    }

    private List<ImportDbInfo> GetZvfDokumenteAsync(UjBauDbContext db) =>
        db.ZvfDokument
            .Select(s => new ImportDbInfo {
                FileName        = s.Dateiname,
                ExportTimestamp = s.ExportTimestamp,
                ImportTimestamp = s.ImportTimestamp
            }).ToList();

    private List<ImportDbInfo> GetUebDokumenteAsync(UjBauDbContext db) =>
        db.UebDokument
            .Select(s => new ImportDbInfo {
                FileName        = s.Dateiname,
                ExportTimestamp = s.ExportTimestamp,
                ImportTimestamp = s.ImportTimestamp
            }).ToList();

    private List<ImportDbInfo> GetFploDokumenteAsync(UjBauDbContext db) =>
        db.FploDokument
            .Select(s => new ImportDbInfo {
                FileName        = s.Dateiname,
                ExportTimestamp = s.ExportTimestamp,
                ImportTimestamp = s.ImportTimestamp
            }).ToList();

    private static void LogDebugJson<T>(string title, T data) {
        if (!Logger.IsDebugEnabled)
            return;

        try {
            var json = JsonSerializer.Serialize(
                data,
#pragma warning disable CA1869
                new JsonSerializerOptions {
                    WriteIndented          = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    Encoder                = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
#pragma warning restore CA1869

            Logger.Debug($"{title}:{Environment.NewLine}{json}");
        }
        catch (Exception ex) {
            Logger.Warn(ex, $"JSON-Debug-Logging fehlgeschlagen ({title})");
        }
    }
}