using BauFahrplanMonitor.Core.Helpers;
using BauFahrplanMonitor.Core.Importer;
using BauFahrplanMonitor.Core.Importer.Helper;
using BauFahrplanMonitor.Core.Services;
using BauFahrplanMonitor.Data;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Core.Jobs;

public sealed class BbpNeoJob {
    private readonly BbpNeoImporter                    _importer;
    private readonly IDbContextFactory<UjBauDbContext> _dbFactory;
    private readonly ConfigService                     _config;

    private readonly object                   _lock = new();
    private          CancellationTokenSource? _cts;

    private readonly BbpNeoJobStatus _status = new();
    public           BbpNeoJobStatus Status => _status;

    public async Task StartAsync(string file, CancellationToken externalToken) {
        lock (_lock) {
            if (_status.State == ImportJobState.Running)
                throw new InvalidOperationException("BBPNeo-Import läuft bereits");

            _status.Reset();
            _status.State       = ImportJobState.Running;
            _status.StartedAt   = DateTime.UtcNow;
            _status.CurrentFile = file;
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);

        try {
            await using var db =
                await _dbFactory.CreateDbContextAsync(_cts.Token);

            var item = new ImportFileItem(
                file,
                DateTime.UtcNow,
                ImportMode.None);

            var progress = new Progress<ImportProgressInfo>(info => {
                lock (_lock) {
                    _status.UpdateFrom(info);
                }
            });

            await _importer.ImportAsync(
                db,
                item,
                progress,
                _cts.Token);

            lock (_lock) {
                _status.MarkFinished(withErrors: _status.Errors > 0);
            }
        }
        catch (OperationCanceledException) {
            lock (_lock) {
                _status.State      = ImportJobState.Cancelled;
                _status.FinishedAt = DateTime.UtcNow;
            }
        }
        catch (Exception) {
            lock (_lock) {
                _status.IncrementErrors();
                _status.State      = ImportJobState.Failed;
                _status.FinishedAt = DateTime.UtcNow;
            }

            throw;
        }
        finally {
            lock (_lock) {
                _status.CurrentFile = null;
            }
        }
    }

    public void RequestCancel() {
        _cts?.Cancel();
    }
}