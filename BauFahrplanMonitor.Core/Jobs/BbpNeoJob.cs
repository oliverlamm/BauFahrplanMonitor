using BauFahrplanMonitor.Core.Helpers;
using BauFahrplanMonitor.Core.Importer;
using BauFahrplanMonitor.Core.Importer.Helper;
using BauFahrplanMonitor.Core.Services;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Core.Jobs;

public sealed class BbpNeoJob {
    private readonly BbpNeoImporter                    _importer;
    private readonly IDbContextFactory<UjBauDbContext> _dbFactory;
    private readonly ConfigService                     _config;

    private readonly BbpNeoJobStatus _status;

    public BbpNeoJobStatus Status => _status;

    public async Task StartAsync(
        string            file,
        CancellationToken token) {
        _status.State = ImportJobState.Running;

        await using var db =
            await _dbFactory.CreateDbContextAsync(token);

        var item = new ImportFileItem(
            file,
            DateTime.Now,
            ImportMode.None);

        _status.Reset();
        _status.State       = ImportJobState.Running;
        _status.StartedAt   = DateTime.Now;
        _status.CurrentFile = item.FilePath;

        var progress = new Progress<ImportProgressInfo>(info => { _status.UpdateFrom(info); });

        try {
            await _importer.ImportAsync(
                db,
                item,
                progress,
                token);

            _status.MarkFinished(withErrors: _status.Errors > 0);
        }
        catch (OperationCanceledException) {
            _status.State = ImportJobState.Aborted;
        }
        catch (Exception) {
            _status.IncrementErrors();
            _status.State = ImportJobState.Failed;
            throw;
        }
    }
    public void RequestCancel() {
        throw new NotImplementedException();
    }
}