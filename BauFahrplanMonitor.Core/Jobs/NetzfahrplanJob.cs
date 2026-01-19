using BauFahrplanMonitor.Core.Helpers;
using BauFahrplanMonitor.Core.Importer;
using BauFahrplanMonitor.Core.Services;
using BauFahrplanMonitor.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BauFahrplanMonitor.Core.Jobs;

public sealed class NetzfahrplanJob {
    private readonly NetzfahrplanImporter              _importer;
    private readonly ConfigService                     _config;
    private readonly ILogger<NetzfahrplanJob>          _logger;
    private readonly IDbContextFactory<UjBauDbContext> _dbFactory;

    private readonly object                        _lock = new();
    private readonly NetzfahrplanJobStatus         _status;
    private          IReadOnlyList<ImportFileItem> _queue = [];

    private          CancellationTokenSource? _cts;
    private volatile bool                     _softCancel;

    public NetzfahrplanJobStatus Status => _status;

    public NetzfahrplanJob(
        NetzfahrplanImporter              importer,
        ConfigService                     config,
        ILogger<NetzfahrplanJob>          logger,
        IDbContextFactory<UjBauDbContext> dbFactory) {

        _importer  = importer;
        _config    = config;
        _logger    = logger;
        _dbFactory = dbFactory;

        var workers =
            _config.Effective.Allgemein.Debugging
                ? 1
                : Math.Max(1, _config.Effective.Allgemein.ImportThreads);

        _status = new NetzfahrplanJobStatus(workers);
    }

    // ----------------------------
    // ScanAsync
    // ----------------------------
    public Task ScanAsync(CancellationToken token) {
        lock (_lock) {
            _status.Reset();
            _status.State = ImportJobState.Scanning;
        }

        var root = _config.Effective.Datei.Importpfad;

        var files = Directory
            .EnumerateFiles(root, "*.xml", SearchOption.AllDirectories)
            .Where(f => f.Contains("KSS", StringComparison.OrdinalIgnoreCase))
            .Select(f => new ImportFileItem(
                f,
                DateTime.UtcNow,
                ImportMode.Kss)) // oder passender Modus
            .ToList();


        lock (_lock) {
            _queue             = files;
            _status.TotalFiles = files.Count;
            _status.QueueCount = files.Count;
            _status.State      = ImportJobState.Scanned;
        }

        _logger.LogInformation(
            "Netzfahrplan Scan abgeschlossen | Dateien={Count}",
            files.Count);

        return Task.CompletedTask;
    }

    // ----------------------------
    // StartImportAsync
    // ----------------------------
    public async Task StartImportAsync(CancellationToken externalToken) {
        lock (_lock) {
            if (_status.State != ImportJobState.Scanned)
                throw new InvalidOperationException("Import nur nach Scan");

            _status.State     = ImportJobState.Running;
            _status.StartedAt = DateTime.Now;
            _softCancel       = false;
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);

        var workerCount =
            _config.Effective.Allgemein.Debugging
                ? 1
                : Math.Max(1, _config.Effective.Allgemein.ImportThreads);

        try {
            await Parallel.ForEachAsync(
                _queue,
                new ParallelOptions {
                    MaxDegreeOfParallelism = workerCount,
                    CancellationToken      = _cts.Token
                },
                async (item, ct) => {
                    // 🔴 HARTER Abbruch
                    ct.ThrowIfCancellationRequested();

                    if (_softCancel)
                        ct.ThrowIfCancellationRequested();

                    var worker = _status.AcquireWorker();

                    // 🔒 Status-Update beim Start
                    lock (_lock) {
                        worker.State       = WorkerState.Working;
                        worker.CurrentFile = item.FilePath;

                        _status.DecrementQueueCount();
                        _status.IncrementActiveWorkers();
                    }

                    try {
                        await using var db =
                            await _dbFactory.CreateDbContextAsync(ct);

                        await _importer.ImportAsync(
                            db,
                            item,
                            progress: null,
                            ct);

                        lock (_lock) {
                            _status.IncrementProcessedFiles();
                            worker.ProcessedItems++;
                        }
                    }
                    catch (OperationCanceledException) {
                        lock (_lock) {
                            worker.State = WorkerState.Canceled;
                        }

                        throw;
                    }
                    catch (Exception ex) {
                        lock (_lock) {
                            worker.State = WorkerState.Error;
                            _status.IncrementErrors();
                        }

                        _logger.LogError(
                            ex,
                            "Netzfahrplan Import fehlgeschlagen | Datei={File}",
                            item.FilePath);
                    }
                    finally {
                        lock (_lock) {
                            _status.DecrementActiveWorkers();

                            worker.State       = WorkerState.Idle;
                            worker.CurrentFile = null;

                            _status.ReleaseWorker(worker);
                        }
                    }
                });
        }
        catch (OperationCanceledException) {
            // gewollt → Cancel
        }

        lock (_lock) {
            if (_softCancel) {
                _status.State = ImportJobState.Cancelled;
            }
            else {
                _status.State =
                    _status.Errors > 0
                        ? ImportJobState.FinishedWithErrors
                        : ImportJobState.Finished;
            }
        }
    }

    // ----------------------------
    // RequestSoftCancel
    // ----------------------------
    public void RequestSoftCancel() {
        lock (_lock) {
            if (_status.State != ImportJobState.Running)
                return;

            _softCancel = true;

            foreach (var w in _status.Workers) {
                if (w.State == WorkerState.Working)
                    w.State = WorkerState.Stopping;
            }
        }

        _cts?.Cancel(); // 🔴 DAS ist der Schlüssel

        _logger.LogInformation("Netzfahrplan Cancel angefordert");
    }
}