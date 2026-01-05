using BauFahrplanMonitor.Core.Helpers;
using BauFahrplanMonitor.Core.Services;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Helpers;
using BauFahrplanMonitor.Importer;
using BauFahrplanMonitor.Importer.Dto;
using BauFahrplanMonitor.Importer.Helper;
using BauFahrplanMonitor.Resolver;
using BauFahrplanMonitor.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BauFahrplanMonitor.Core.Jobs;

public sealed class ZvFExportJob {
    private readonly ZvFExportImporter                 _importer;
    private readonly ConfigService                     _config;
    private readonly IDbContextFactory<UjBauDbContext> _dbFactory;
    private readonly ILogger<ZvFExportJob>             _logger;
    private readonly ZvFExportScanService              _scanService;
    private          CancellationTokenSource?          _cts    = new();
    private readonly object                            _lock   = new();
    private readonly ZvFExportJobStatus                _status = new();
    private          IReadOnlyList<ImportFileItem>     _queue  = Array.Empty<ImportFileItem>();
    public           ZvFExportJobStatus                Status { get; } = new();
    private          int                               _regionWarmupDone = 0;
    private readonly SharedReferenceResolver           _resolver;

    public ZvFExportJob(
        ZvFExportImporter                 importer,
        ConfigService                     config,
        ZvFExportScanService              scanService,
        ILogger<ZvFExportJob>             logger,
        IDbContextFactory<UjBauDbContext> dbFactory) {

        _importer    = importer;
        _config      = config;
        _dbFactory   = dbFactory;
        _logger      = logger;
        _scanService = scanService;
    }

    // --------------------------------------------------
    // START
    // --------------------------------------------------
    public async Task StartAsync(
        ImportRunMode     runMode,
        ZvFFileFilter     filter,
        CancellationToken externalToken) {

        if (Status.State is ImportJobState.Running or ImportJobState.Scanning)
            throw new InvalidOperationException("ZvFExportJob läuft bereits.");

        _cts = CancellationTokenSource
            .CreateLinkedTokenSource(externalToken);

        var token = _cts.Token;

        Status.State     = ImportJobState.Scanning;
        Status.StartedAt = DateTime.UtcNow;

        try {
            // 1️⃣ Scan
            var files = ScanFiles(filter);
            Status.TotalFiles = files.Count;

            if (runMode == ImportRunMode.Scan) {
                Status.State = ImportJobState.Finished;
                return;
            }

            // 2️⃣ Import
            _logger.LogInformation(
                "ZvFExport Import gestartet | Files={Count}",
                files.Count);
            Status.State = ImportJobState.Running;

            var progress = new Progress<ImportProgressInfo>(_ => { Status.IncrementProcessedFiles(); });
            foreach (var file in files) {
                token.ThrowIfCancellationRequested();
                _logger.LogDebug("Importiere Datei {File}", file);

                var mode = ImportModeResolver.Resolve(file);
                if (mode == ImportMode.None) {
                    _logger.LogDebug("Datei übersprungen (unbekannter Typ): {File}", file);
                    continue;
                }

                await using var db = await _dbFactory.CreateDbContextAsync(token);

                var item = new ImportFileItem(
                    file,
                    DateTime.UtcNow,
                    mode);

                await _importer.ImportAsync(
                    db,
                    item,
                    progress,
                    token);
            }

            _logger.LogInformation("ZvFExport Import abgeschlossen");
            Status.State = ImportJobState.Finished;
        }
        catch (OperationCanceledException) {
            Status.State = ImportJobState.Aborted;
        }
        catch {
            Status.State = ImportJobState.Failed;
            throw;
        }
    }

    // --------------------------------------------------
    // CANCEL
    // --------------------------------------------------
    public void Cancel() => _cts?.Cancel();

    // --------------------------------------------------
    // Scan / Filter
    // --------------------------------------------------
    private List<string> ScanFiles(ZvFFileFilter filter) {
        var root = _config.Effective.Datei.Importpfad;

        _logger.LogInformation(
            "ZvFExport Scan gestartet | Importpfad='{Importpfad}' | Filter={Filter}",
            root,
            filter);

        if (string.IsNullOrWhiteSpace(root)) {
            _logger.LogError(
                "ZvFExport Scan abgebrochen: Importpfad ist leer");
            throw new InvalidOperationException(
                "Config.Effective.Datei.Importpfad ist nicht gesetzt.");
        }

        if (!Directory.Exists(root)) {
            _logger.LogError(
                "ZvFExport Scan abgebrochen: Importpfad existiert nicht | {Importpfad}",
                root);
            throw new InvalidOperationException(
                $"Importpfad existiert nicht: {root}");
        }

        var files = Directory
            .EnumerateFiles(root, "*.xml", SearchOption.AllDirectories)
            .ToList();

        _logger.LogInformation(
            "ZvFExport Scan: {Count} Dateien gefunden",
            files.Count);

        return files;
    }

    // --------------------------------------------------
    // Scan
    // --------------------------------------------------
    public async Task StartScanAsync(
        ZvFFileFilter     filter,
        CancellationToken token) {
        ScanStat stat;

        await EnsureRegionWarmupAsync(token);
        lock (_lock) {
            _status.State     = ImportJobState.Scanning;
            _status.StartedAt = DateTime.UtcNow;

            stat             = new ScanStat();
            _status.ScanStat = stat; // 🔑 EIN Objekt

            _queue             = Array.Empty<ImportFileItem>();
            _status.QueueCount = 0;
            _status.TotalFiles = 0;
            _status.ResetErrors();
        }

        // Phase A
        var candidates = _scanService.PreScan(filter, token);

        lock (_lock) {
            _status.TotalFiles = candidates.Count;
        }

        // Phase B – WICHTIG: dieselbe Stat-Instanz übergeben
        var queue = await _scanService.ValidateAsync(
            candidates,
            stat,
            token);

        lock (_lock) {
            _queue             = queue;
            _status.QueueCount = queue.Count;
            _status.State      = ImportJobState.Scanned;
        }
    }

    public ZvFExportJobStatus GetStatus() {
        lock (_lock) {
            return _status;
        }
    }

    public async Task StartImportAsync(CancellationToken externalToken) {
        await EnsureRegionWarmupAsync(externalToken);
        lock (_lock) {
            if (_status.State is not ImportJobState.Scanned)
                throw new InvalidOperationException(
                    "Import kann nur nach abgeschlossenem Scan gestartet werden.");

            _status.State     = ImportJobState.Running;
            _status.StartedAt = DateTime.UtcNow;
            _status.ResetErrors();
            _status.ResetProcessedFiles();
        }

        _logger.LogInformation("ZvFExportJob: Import gestartet | Queue={Count}", _queue.Count);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken, _cts.Token);

        try {
            await RunWorkersAsync(linkedCts.Token);

            lock (_lock) {
                _status.State = _status.Errors > 0
                    ? ImportJobState.FinishedWithErrors
                    : ImportJobState.Finished;
            }

            _logger.LogInformation("ZvFExportJob: Import abgeschlossen");
        }
        catch (OperationCanceledException) {
            lock (_lock) {
                _status.State = ImportJobState.Aborted;
            }

            _logger.LogWarning("ZvFExportJob: Import abgebrochen");
        }
        catch (Exception ex) {
            lock (_lock) {
                _status.State = ImportJobState.Failed;
                _status.IncrementErrors();
            }

            _logger.LogError(ex, "ZvFExportJob: Fehler beim Import");
        }
    }

    private async Task RunWorkersAsync(CancellationToken token) {
        foreach (var item in _queue) {
            token.ThrowIfCancellationRequested();

            try {
                await ImportOneAsync(item, token);
                _status.IncrementProcessedFiles();
            }
            catch (Exception ex) {
                _status.IncrementErrors();

                _logger.LogError(ex,
                    "Import fehlgeschlagen | Datei={File}",
                    item.FilePath);

                if (_config.Effective.Allgemein.StopAfterException)
                    throw;
            }
        }
    }

    private async Task ImportOneAsync(
        ImportFileItem    item,
        CancellationToken token) {
        using (_logger.BeginScope(new Dictionary<string, object> {
                   ["ImportFile"] = item.FilePath
               })) {
            _logger.LogDebug("Import gestartet");

            await using var db = await _dbFactory.CreateDbContextAsync(token);

            var lastLog = DateTime.MinValue;

            var progress = new Progress<ImportProgressInfo>(p => {
                _status.IncrementProcessedFiles();

                if (_logger.IsEnabled(LogLevel.Debug)
                    && DateTime.UtcNow - lastLog > TimeSpan.FromSeconds(1)) {

                    lastLog = DateTime.UtcNow;

                    _logger.LogDebug(
                        "Import läuft | {File} | {Current}/{Total}",
                        item.FilePath,
                        p.ProcessedItems,
                        p.TotalItems);
                }
            });


            _status.CurrentFile = item.FilePath;

            await _importer.ImportAsync(
                db,
                item,
                progress,
                token);

            _status.CurrentFile = null;
            _logger.LogDebug("Import abgeschlossen");
        }
    }

    private async Task EnsureRegionWarmupAsync(CancellationToken token) {
        // 🔑 exakt einmal pro App-Lauf
        if (Interlocked.Exchange(ref _regionWarmupDone, 1) == 1)
            return;

        _logger.LogInformation("Region-Resolver WarmUp gestartet");

        await using var db = await _dbFactory.CreateDbContextAsync(token);
        await _resolver.WarmUpRegionCacheAsync(db, token);

        _logger.LogInformation("Region-Resolver WarmUp abgeschlossen");
    }
}