using BauFahrplanMonitor.Core.Helpers;
using BauFahrplanMonitor.Core.Importer;
using BauFahrplanMonitor.Core.Importer.Dto;
using BauFahrplanMonitor.Core.Importer.Helper;
using BauFahrplanMonitor.Core.Resolver;
using BauFahrplanMonitor.Core.Services;
using BauFahrplanMonitor.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BauFahrplanMonitor.Core.Jobs;

public sealed class ZvFExportJob {
    private readonly ZvFExportImporter                 _importer;
    private readonly ConfigService                     _config;
    private readonly IDbContextFactory<UjBauDbContext> _dbFactory;
    private readonly ILogger<ZvFExportJob>             _logger;
    private readonly ZvFExportScanService              _scanService;
    private          CancellationTokenSource?          _cts  = new();
    private readonly object                            _lock = new();
    private readonly ZvFExportJobStatus                _status;
    private          IReadOnlyList<ImportFileItem>     _queue = [];
    private          int                               _regionWarmupDone;
    private readonly SharedReferenceResolver           _resolver;
    private volatile bool                              _initialized;
    private          int                               _softCancelRequested;

    public ZvFExportJobStatus Status => _status;


    public ZvFExportJob(
        ZvFExportImporter                 importer,
        ConfigService                     config,
        ZvFExportScanService              scanService,
        SharedReferenceResolver           resolver,
        ILogger<ZvFExportJob>             logger,
        IDbContextFactory<UjBauDbContext> dbFactory) {

        _importer    = importer;
        _config      = config;
        _dbFactory   = dbFactory;
        _logger      = logger;
        _scanService = scanService;
        _resolver    = resolver;

        var workerCount =
            _config.Effective.Allgemein.Debugging
                ? 1
                : Math.Max(1, _config.Effective.Allgemein.ImportThreads);

        _status = new ZvFExportJobStatus(workerCount);
    }

    // --------------------------------------------------
    // Cancel
    // --------------------------------------------------
    public void Cancel() => _cts?.Cancel();

    // --------------------------------------------------
    // StartScanAsync
    // --------------------------------------------------
    private async Task StartScanAsync(
        ZvFFileFilter     filter,
        CancellationToken token) {
        ScanStat stat;

        lock (_lock) {
            _status.State     = ImportJobState.Scanning;
            _status.StartedAt = DateTime.UtcNow;

            stat             = new ScanStat();
            _status.ScanStat = stat;

            _queue = [];

            _status.ResetScan(0);
        }

        // -----------------------------
        // Kandidaten sammeln
        // -----------------------------
        var candidates = _scanService.PreScan(filter, token);

        lock (_lock) {
            _status.ResetScan(candidates.Count);
        }

        // -----------------------------
        // Scan-Fortschritt zählen
        // -----------------------------
        var progress = new Progress<string>(file => {
            _status.CurrentFile = file;
            _status.IncrementScanProcessed();
        });

        // -----------------------------
        // Validieren
        // -----------------------------
        var queue = await _scanService.ValidateAsync(
            candidates,
            stat,
            progress,
            token);

        lock (_lock) {
            _queue                   = queue;
            _status.ImportTotalItems = queue.Count;

            if (_status.State is ImportJobState.Scanning or ImportJobState.Starting) {
                _status.State = ImportJobState.Scanned;
            }
        }
    }

    // --------------------------------------------------
    // StartImportAsync
    // --------------------------------------------------
    public async Task StartImportAsync(CancellationToken externalToken) {

        // --------------------------------------------------
        // Vorbedingungen
        // --------------------------------------------------
        await EnsureRegionWarmupAsync(externalToken);

        lock (_lock) {
            if (_status.State is not ImportJobState.Scanned)
                throw new InvalidOperationException(
                    "Import kann nur nach abgeschlossenem Scan gestartet werden.");

            // 🔑 Soft-Cancel zurücksetzen
            _softCancelRequested = 0;

            // 🔑 Import-Start-Zustand
            _status.State     = ImportJobState.Running;
            _status.StartedAt = DateTime.Now;

            // 🔑 Import-Fortschritt sauber zurücksetzen
            _status.ResetImport(_status.ImportTotalItems);

            // 🔑 Worker-Zähler sauber zurücksetzen
            _status.ResetActiveWorkers();
        }

        _logger.LogInformation(
            "ZvFExportJob: Import gestartet | Queue={Count}",
            _queue.Count);

        // --------------------------------------------------
        // Cancellation sauber aufsetzen
        // --------------------------------------------------
        using var linkedCts =
            CancellationTokenSource.CreateLinkedTokenSource(externalToken);

        try {
            // --------------------------------------------------
            // Import ausführen
            // --------------------------------------------------
            await RunWorkersAsync(linkedCts.Token);

            // --------------------------------------------------
            // Soft-Cancel Nachbereitung
            // --------------------------------------------------
            if (_softCancelRequested == 1) {
                lock (_lock) {
                    foreach (var w in _status.Workers) {
                        if (w.State == WorkerState.Stopping)
                            w.State = WorkerState.Idle;
                    }

                    _status.State = ImportJobState.Aborted;
                }

                _logger.LogInformation(
                    "ZvFExportJob: Soft-Cancel abgeschlossen");

                return;
            }

            // --------------------------------------------------
            // Abschlussstatus
            // --------------------------------------------------
            lock (_lock) {
                _status.State =
                    _status.ImportErrorItems > 0
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
                _status.IncrementImportErrors();
            }

            _logger.LogError(ex, "ZvFExportJob: Fehler beim Import");
        }
    }

    // --------------------------------------------------
    // RunWorkersAsync
    // --------------------------------------------------
    private async Task RunWorkersAsync(CancellationToken token) {

        var workerCount =
            _config.Effective.Allgemein.Debugging
                ? 1
                : Math.Max(1, _config.Effective.Allgemein.ImportThreads);

        _logger.LogInformation(
            "ZvFExportJob: Starte Import mit {Workers} Worker(n)",
            workerCount);

        // 🔑 Startzustand
        _status.ResetActiveWorkers();

        await Parallel.ForEachAsync(
            _queue,
            new ParallelOptions {
                MaxDegreeOfParallelism = workerCount,
                CancellationToken      = token
            },
            async (item, ct) => {

                // Soft-Cancel: keine neuen Jobs beginnen
                if (_softCancelRequested == 1)
                    return;

                var worker                = _status.AcquireWorker();
                var completedSuccessfully = false;

                try {
                    worker.CurrentFile = item.FilePath;

                    await ImportOneAsync(worker, item, ct);

                    // ✅ Worker
                    worker.ProcessedItems++;

                    // ✅ Job
                    _status.IncrementImportProcessed();
                    completedSuccessfully = true;
                }
                catch (OperationCanceledException) {
                    worker.State = WorkerState.Canceled;
                    throw;
                }
                catch (Exception ex) {
                    worker.State = WorkerState.Error;

                    // ✅ Worker
                    worker.Errors++;

                    // ✅ Job
                    _status.IncrementImportErrors();

                    _logger.LogError(ex,
                        "Import fehlgeschlagen | Datei={File}",
                        item.FilePath);
                    
                    if (_config.Effective.Allgemein.StopAfterException) {
                        worker.State         = WorkerState.Canceled;
                        _softCancelRequested = 1;
                        throw;
                    }
                }
                finally {
                    lock (_lock) {
                        if (_softCancelRequested == 1 &&
                            worker.State         == WorkerState.Working) {

                            worker.State = WorkerState.Stopping;
                        }
                        else if (completedSuccessfully) {
                            // ❗ NUR bei Erfolg freigeben
                            _status.ReleaseWorker(worker);
                        }
                    }
                }
            });
    }
    
    // --------------------------------------------------
    // ImportOneAsync
    // --------------------------------------------------
    private async Task ImportOneAsync(
        ImportWorkerStatus worker,
        ImportFileItem     item,
        CancellationToken  token) {
        token.ThrowIfCancellationRequested();

        using (_logger.BeginScope(new Dictionary<string, object> {
                   ["ImportFile"] = item.FilePath
               })) {

            _logger.LogDebug("Import gestartet");

            await using var db =
                await _dbFactory.CreateDbContextAsync(token);

            var lastLog = DateTime.MinValue;

            var progress = new Progress<ImportProgressInfo>(p => {

                // 🔑 1) IMMER in den Worker-Status schreiben
                lock (_lock) {
                    worker.ProgressMessage = p.StepText;
                    worker.CurrentFile     = p.FileName;
                }

                // 🔑 2) OPTIONAL: Logging wie bisher
                if (!_logger.IsEnabled(LogLevel.Debug))
                    return;

                if (DateTime.UtcNow - lastLog < TimeSpan.FromSeconds(1))
                    return;

                lastLog = DateTime.UtcNow;

                _logger.LogDebug(
                    "Import läuft | {File} | {Step}",
                    item.FilePath,
                    p.StepText);
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

    // --------------------------------------------------
    // EnsureInitializedAsync
    // --------------------------------------------------
    private async Task EnsureInitializedAsync(CancellationToken token) {
        if (_initialized)
            return;

        await EnsureRegionWarmupAsync(token);
        _initialized = true;
    }

    // --------------------------------------------------
    // EnsureRegionWarmupAsync
    // --------------------------------------------------
    private async Task EnsureRegionWarmupAsync(CancellationToken token) {
        // 🔑 exakt einmal pro App-Lauf
        if (Interlocked.Exchange(ref _regionWarmupDone, 1) == 1)
            return;

        _logger.LogInformation("Region-Resolver WarmUp gestartet");

        await using var db = await _dbFactory.CreateDbContextAsync(token);
        await _resolver.WarmUpRegionCacheAsync(db, token);

        _logger.LogInformation("Region-Resolver WarmUp abgeschlossen");
    }

    // --------------------------------------------------
    // TriggerScanAsync
    // --------------------------------------------------
    public async Task TriggerScanAsync(
        ZvFFileFilter     filter,
        CancellationToken externalToken) {
        lock (_lock) {
            if (_status.State is ImportJobState.Scanning
                or ImportJobState.Running
                or ImportJobState.Starting)
                return;

            // 🔑 klarer Übergang
            _status.State = ImportJobState.Starting;
        }

        await EnsureInitializedAsync(externalToken);

        // 🔑 Scan bekommt seinen eigenen CTS
        var scanCts = CancellationTokenSource
            .CreateLinkedTokenSource(externalToken);

        _ = Task.Run(async () => {
            try {
                await StartScanAsync(filter, scanCts.Token);
            }
            catch (OperationCanceledException) {
                lock (_lock) {
                    _status.State = ImportJobState.Aborted;
                }

                _logger.LogWarning("ZvFExportJob: Scan abgebrochen");
            }
            catch (Exception ex) {
                lock (_lock) {
                    _status.State = ImportJobState.Failed;
                    _status.IncrementImportErrors();
                }

                _logger.LogError(ex, "ZvFExportJob: Scan fehlgeschlagen");
            }
        }, CancellationToken.None); // 🔑 Task selbst nicht abbrechen
    }

    // --------------------------------------------------
    // RequestCancel
    // --------------------------------------------------
    public bool RequestCancel() {
        // atomar: war schon gesetzt?
        if (Interlocked.Exchange(ref _softCancelRequested, 1) == 1)
            return false;

        _logger.LogInformation("ZvFExportJob: Cancel angefordert (Soft)");

        lock (_lock) {
            if (_status.State == ImportJobState.Running)
                _status.State = ImportJobState.Stopping;
        }

        return true;
    }
}