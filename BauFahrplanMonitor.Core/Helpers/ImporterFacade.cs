using BauFahrplanMonitor.Core.Jobs;

namespace BauFahrplanMonitor.Core.Helpers;

public sealed class ImporterFacade(
    ZvFExportJob zvfJob) {

    private readonly ZvFExportJob _zvfJob = zvfJob;

    // -----------------------------
    // Scan
    // -----------------------------
    public Task StartZvFExportScanAsync(
        ZvFFileFilter     filter,
        CancellationToken token)
        => _zvfJob.TriggerScanAsync(filter, token);

    // -----------------------------
    // Import
    // -----------------------------
    public Task StartZvFExportImportAsync(
        CancellationToken token)
        => _zvfJob.StartImportAsync(token);

    // -----------------------------
    // Cancel
    // -----------------------------
    public void CancelZvFExport()
        => _zvfJob.RequestCancel();

    // -----------------------------
    // Status
    // -----------------------------
    public ZvFExportJobStatus GetZvFExportStatus()
        => _zvfJob.Status;
}
