using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Avalonia.Media;
using Avalonia.Threading;
using BauFahrplanMonitor.Core.Helpers;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Helpers;
using BauFahrplanMonitor.Importer.Dto;
using BauFahrplanMonitor.Importer.Helper;
using BauFahrplanMonitor.Interfaces;
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

    private static readonly ConcurrentDictionary<string, SemaphoreSlim> FileLocks
        = new();

    private Dictionary<string, ImportDbInfo>? _dbImportCache;

    private int _importErrorCount = 0;

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
            ImporterStatus.Bereit                    => Brushes.Gray,
            ImporterStatus.Scannen                   => Brushes.RoyalBlue,
            ImporterStatus.Importieren               => Brushes.DodgerBlue,
            ImporterStatus.Abgeschlossen             => Brushes.ForestGreen,
            ImporterStatus.AbgeschlossenMitFehlern   => Brushes.DarkOrange,
            ImporterStatus.Abbruch                   => Brushes.Red,
            ImporterStatus.Fehler                    => Brushes.IndianRed,
            ImporterStatus.AbgeschlossenMitException => Brushes.DarkRed,
            _                                        => Brushes.Gray
        };

    public string StatusText =>
        Status switch {
            ImporterStatus.Bereit                    => "Bereit",
            ImporterStatus.Scannen                   => "Scanne…",
            ImporterStatus.Importieren               => "Importiere…",
            ImporterStatus.Abgeschlossen             => "Abgeschlossen",
            ImporterStatus.AbgeschlossenMitFehlern   => "Abgeschlossen mit Fehlern",
            ImporterStatus.AbgeschlossenMitException => "Exception",
            ImporterStatus.Abbruch                   => "Abbruch",
            ImporterStatus.Fehler                    => "Fehler",
            _                                        => "Unbekannt"
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
        ImporterTyp                       importerTyp) {
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
        var sw = new Stopwatch();

        try {
            sw.Start();
            await Task.Run(() => BuildQueueWithStatusAsync(_scanCts.Token), _scanCts.Token);

            TotalFiles = _importQueue.Count;
            Status     = ImporterStatus.Abgeschlossen;
            sw.Stop();
            Logger.Info($"Scan hat {sw.ToString()} gedauert");
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

            if (_importErrorCount > 0) {
                Status = ImporterStatus.AbgeschlossenMitFehlern;
                _statusMessages.Warning(
                    $"Import endet mit Fehlern ({_importErrorCount} Datei(en))");
            }
            else {
                Status = ImporterStatus.Abgeschlossen;
                _statusMessages.Success("Erfolgreich importiert");
            }

            // 🔄 Cache NUR bei regulärem Import neu aufbauen
            await BuildImportCacheAsync();
        }
        catch (StopAfterExceptionException) {
            // 🛑 kontrollierter Debug-Abbruch
            Status = ImporterStatus.AbgeschlossenMitException;
            _statusMessages.Warning(
                "Import nach erster Exception abgebrochen (Debug)");
        }
        catch (OperationCanceledException) {
            // 🛑 Benutzer- oder globaler Abbruch
            Status = ImporterStatus.Abbruch;
            _statusMessages.Warning("Import wurde abgebrochen");
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
        if (ImporterTyp == ImporterTyp.Direkt) {
            BuildQueueDirect(token);
            return;
        }

        await BuildQueueZvFExportAsync(token);
    }

    private void BuildQueueDirect(CancellationToken token) {
        Status = ImporterStatus.Scannen;

        var files = ScanFilesDirect();

        Dispatcher.UIThread.Invoke(() => {
            TotalFiles     = files.Count;
            ProcessedFiles = 0;
        });

        _importQueue.Clear();

        var processed = 0;

        foreach (var file in files) {
            token.ThrowIfCancellationRequested();

            // 🔑 KEINE Prüfungen, KEINE DB, KEINE Sortierung
            var item = new ImportFileItem(
                file,
                File.GetCreationTimeUtc(file),
                ResolveFileType(file)
            );

            _importQueue.Enqueue(item);

            processed++;
            if (processed % 10 == 0 || processed == files.Count) {
                Dispatcher.UIThread.Invoke(() => { ProcessedFiles = processed; });
            }
        }

        Dispatcher.UIThread.Invoke(() => {
            OnPropertyChanged(nameof(QueueCount));
            OnPropertyChanged(nameof(CanStart));
            _statusMessages.Success($"Direkt-Import: {files.Count} Dateien in Queue");
        });

        Logger.Info($"[Direkt] {files.Count} Dateien ungefiltert in Queue");
    }

    private async Task BuildQueueZvFExportAsync(CancellationToken token) {
        var files = ImporterTyp switch {
            ImporterTyp.Direkt    => ScanFilesDirect(),
            ImporterTyp.ZvFExport => ScanFilesZvFExport(),
            _                     => []
        };

        LogDebugJson(
            "Scan-Ergebnis (Dateiliste)",
            files.Select(f => new {
                File = f
            }).ToList());

        Dispatcher.UIThread.Invoke(() => {
            TotalFiles     = files.Count;
            ProcessedFiles = 0;
        });

        await BuildImportCacheAsync();

        var maxDegree = _configService.Effective.Allgemein.ImportThreads;
        var processed = 0;
        var stat      = new ScanStat();
        var items     = new ConcurrentBag<ImportFileItem>();

        await Parallel.ForEachAsync(
            files,
            new ParallelOptions {
                MaxDegreeOfParallelism = maxDegree,
                CancellationToken      = token
            },
            async (file, ct) => {
                ct.ThrowIfCancellationRequested();

                var mode = ResolveFileType(file);
                var item = TryCreateImportItem(file);

                // -------------------------
                // Statistik + Queue
                // -------------------------
                if (item != null) {
                    items.Add(item);

                    switch (mode) {
                        case ImportMode.ZvF:
                            Interlocked.Increment(ref stat.ZvF_New);
                            break;
                        case ImportMode.UeB:
                            Interlocked.Increment(ref stat.UeB_New);
                            break;
                        case ImportMode.Fplo:
                            Interlocked.Increment(ref stat.Fplo_New);
                            break;
                        case ImportMode.Kss:
                            Interlocked.Increment(ref stat.Kss_New);
                            break;
                    }
                }
                else {
                    switch (mode) {
                        case ImportMode.ZvF:
                            Interlocked.Increment(ref stat.ZvF_Imported);
                            break;
                        case ImportMode.UeB:
                            Interlocked.Increment(ref stat.UeB_Imported);
                            break;
                        case ImportMode.Fplo:
                            Interlocked.Increment(ref stat.Fplo_Imported);
                            break;
                        case ImportMode.Kss:
                            Interlocked.Increment(ref stat.Kss_Imported);
                            break;
                    }
                }

                // -------------------------
                // Fortschritt (throttled)
                // -------------------------
                var current = Interlocked.Increment(ref processed);

                if (current % 10 == 0 || current == files.Count) {
                    await Dispatcher.UIThread.InvokeAsync(() => {
                        ProcessedFiles = current;
                        Status         = ImporterStatus.Scannen;
                        IsScanning     = true;
                    });
                }
            });

        // 2️⃣ SORTIERUNG: älteste zuerst
        var sortedItems = items
            .OrderBy(i => i.SortTimestamp)
            .ToList();

        // 3️⃣ Queue leeren (Sicherheit)
        _importQueue.Clear();

        // 4️⃣ SORTIERT enqueuen
        foreach (var item in sortedItems) {
            _importQueue.Enqueue(item);
        }

        // 5️⃣ UI informieren
        Dispatcher.UIThread.Invoke(() => {
            OnPropertyChanged(nameof(QueueCount));
            OnPropertyChanged(nameof(CanStart));
        });

        var statusText = ImporterTyp switch {
            ImporterTyp.ZvFExport =>
                $"Gefundene Dateien: "                                       +
                $"ZvF: {stat.ZvF_New} neu, {stat.ZvF_Imported} importiert, " +
                $"ÜB: {stat.UeB_New} neu, {stat.UeB_Imported} importiert, "  +
                $"Fplo: {stat.Fplo_New} neu, {stat.Fplo_Imported} importiert",

            ImporterTyp.Direkt =>
                $"Gefundene Dateien: " +
                $"KSS: {stat.Kss_New} neu, {stat.Kss_Imported} importiert",

            _ => "Gefundene Dateien"
        };

        Dispatcher.UIThread.Invoke(() => { _statusMessages.Success(statusText); });

        Logger.Info(statusText);

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
        while (!token.IsCancellationRequested &&
               _importQueue.TryDequeue(out var item)) {
            var totalZuege     = 0;
            var totalEntfallen = 0;
            var doneZuege      = 0;
            var doneEntfallen  = 0;

            var progress = new Progress<ImportProgressInfo>(info => {
                Dispatcher.UIThread.Post(() => {
                    threadVm.FileName      = info.FileName;
                    threadVm.Status        = ImportThreadStatus.Importieren;
                    threadVm.StatusMessage = info.Step;

                    threadVm.ThreadProgressText =
                        $"{info.StepIndex} / {info.TotalSteps}";

                    if (!info.SubIndex.HasValue || !info.SubTotal.HasValue)
                        return;

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

                    if (total > 0)
                        threadVm.ThreadProgress = (int)(done * 100.0 / total);
                });
            });

            try {
                await using var db       = await _dbFactory.CreateDbContextAsync(token);
                var             importer = _importerFactory.GetImporter(_importerTyp);

                var fileKey = Path.GetFileName(item.FilePath);
                var sem = FileLocks.GetOrAdd(
                    fileKey,
                    _ => new SemaphoreSlim(1, 1));

                await sem.WaitAsync(token);
                try {
                    await importer.ImportAsync(db, item, progress, token);
                    db.ChangeTracker.Clear();
                }
                finally {
                    sem.Release();
                    FileLocks.TryRemove(fileKey, out _);
                }

                // ✅ Erfolgreich verarbeitet
                await Dispatcher.UIThread.InvokeAsync(() => {
                    threadVm.ThreadProgress     = 0;
                    threadVm.ThreadProgressText = string.Empty;
                });
            }
            catch (OperationCanceledException) {
                // 🔑 globaler Abbruch
                await Dispatcher.UIThread.InvokeAsync(() => {
                    threadVm.Status        = ImportThreadStatus.Abbruch;
                    threadVm.StatusMessage = "Abgebrochen";
                });
                throw;
            }
            catch (StopAfterExceptionException) {
                Logger.Fatal(
                    "StopAfterException aktiv → Import & Scan werden gestoppt");

                _importCts?.Cancel();
                _scanCts?.Cancel();

                await Dispatcher.UIThread.InvokeAsync(() => {
                    threadVm.Status = ImportThreadStatus.AbbruchnachException;
                    threadVm.StatusMessage =
                        "Abbruch nach erster Exception (Debug)";
                });

                return; // ✅ sauberer Thread-Abbruch
            }
            catch (Exception ex) {
                Interlocked.Increment(ref _importErrorCount);

                Logger.Error(
                    ex,
                    "Fehler beim Import der Datei {0}",
                    item.FilePath);

                await Dispatcher.UIThread.InvokeAsync(() => {
                    threadVm.Status        = ImportThreadStatus.Fehler;
                    threadVm.StatusMessage = ex.Message;
                });

                continue;
            }

            // Coordinator: Overall (nur wenn Datei abgeschlossen)
            await Dispatcher.UIThread.InvokeAsync(() => {
                ProcessedFiles++;
                OnPropertyChanged(nameof(OverallProgress));
                OnPropertyChanged(nameof(OverallProgressText));
            });
        }

        // Thread ist fertig (Queue leer oder globaler Abbruch)
        await Dispatcher.UIThread.InvokeAsync(() => {
            threadVm.Status        = ImportThreadStatus.Beendet;
            threadVm.FileName      = string.Empty;
            threadVm.StatusMessage = "Keine Dateien mehr";
        });
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
        if (name.StartsWith("ÜB", StringComparison.OrdinalIgnoreCase)) return ImportMode.UeB;
        return name.StartsWith("Fplo", StringComparison.OrdinalIgnoreCase) ? ImportMode.Fplo : ImportMode.None;
    }

    private IEnumerable<string> GetZvFSearchPatterns() {
        if (FilterAll)
            return new[] {
                "ZvF*.xml", "ÜB*.xml", "Fplo*.xml"
            };

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

    private void CleanupFileAfterSuccessfulImport(string filePath) {
        var cfg = _configService.Effective.Datei;

        if (cfg is {
                Archivieren: false,
                NachImportLoeschen: false
            })
            return;

        try {
            if (cfg.Archivieren) {
                Directory.CreateDirectory(cfg.Archivpfad);

                var target = Path.Combine(
                    cfg.Archivpfad,
                    Path.GetFileName(filePath));

                File.Copy(filePath, target, overwrite: true);
            }

            if (cfg.NachImportLoeschen) {
                File.Delete(filePath);
            }

            Logger.Info($"[Direkt][Cleanup] {Path.GetFileName(filePath)}");
        }
        catch (Exception ex) {
            // 🔑 Import war erfolgreich → Cleanup-Fehler ist sekundär
            Logger.Warn(
                ex,
                "[Direkt][Cleanup] Fehler bei Datei {0}",
                filePath);
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