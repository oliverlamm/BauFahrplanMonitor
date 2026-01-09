using BauFahrplanMonitor.Core.Helpers;
using BauFahrplanMonitor.Core.Importer;
using BauFahrplanMonitor.Core.Jobs;
using BauFahrplanMonitor.Helpers;
using BauFahrplanMonitor.Data;
using Microsoft.Extensions.Logging;

namespace BauFahrplanMonitor.Core.Tools;

public sealed class ImporterFacade(
    ZvFExportJob zvfJob) {

    private readonly ZvFExportJob _zvfJob = zvfJob;

    public Task StartZvFExportAsync(
        ZvFImportCommand  command,
        ZvFFileFilter     filter,
        CancellationToken token) {

        return _zvfJob.StartAsync(
            command == ZvFImportCommand.Scan
                ? ImportRunMode.Scan
                : ImportRunMode.Import,
            filter,
            token);
    }

    public ZvFExportJobStatus GetZvFExportStatus()
        => _zvfJob.Status;

    public void CancelZvFExport()
        => _zvfJob.Cancel();
}