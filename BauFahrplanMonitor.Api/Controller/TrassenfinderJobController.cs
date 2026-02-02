using BauFahrplanMonitor.Api.Jobs;
using BauFahrplanMonitor.Core.Interfaces;
using BauFahrplanMonitor.Core.Jobs;
using Microsoft.AspNetCore.Mvc;

namespace BauFahrplanMonitor.Api.Controller;

[ApiController]
[Route("api/trassenfinder")]
public sealed class TrassenfinderJobController : ControllerBase
{
    private readonly TrassenfinderJobStore _jobs;
    private readonly ITrassenfinderRefreshJob _refreshJob;

    public TrassenfinderJobController(
        TrassenfinderJobStore jobs,
        ITrassenfinderRefreshJob refreshJob)
    {
        this._jobs       = jobs;
        this._refreshJob = refreshJob;
    }

    // ------------------------------------------------------------
    // Startet einen Infrastruktur-Refresh-Job
    // ------------------------------------------------------------
    [HttpPost("infrastruktur/{id:long}/refresh")]
    public IActionResult RefreshInfrastruktur(long id)
    {
        var job               = this._jobs.Create();
        var cancellationToken = HttpContext.RequestAborted;

        _ = Task.Run(() =>
            RunRefreshJobAsync(job.JobId, id, cancellationToken)
        );

        return Accepted(new { jobId = job.JobId });
    }

    // ------------------------------------------------------------
    // Liefert Job-Status (Polling)
    // ------------------------------------------------------------
    [HttpGet("jobs/{jobId}")]
    public IActionResult GetJob(string jobId)
    {
        if (!this._jobs.TryGet(jobId, out var job))
            return NotFound();

        return Ok(job);
    }

    // ------------------------------------------------------------
    // Hintergrund-Logik
    // ------------------------------------------------------------
    private async Task RunRefreshJobAsync(
        string jobId,
        long infrastrukturId,
        CancellationToken token)
    {
        try
        {
            var progress = new Progress<TrassenfinderInfraStatus>(p =>
                this._jobs.Update(
                    jobId,
                    TrassenfinderJobState.Running,
                    p.Percent,
                    p.Message
                )
            );

            await this._refreshJob.RefreshInfrastrukturAsync(
                infrastrukturId,
                progress,
                token
            );

            this._jobs.Update(jobId, TrassenfinderJobState.Done, 100, "Fertig");
        }
        catch (OperationCanceledException)
        {
            this._jobs.Update(jobId, TrassenfinderJobState.Failed, 0, "Abgebrochen");
        }
        catch (Exception ex)
        {
            this._jobs.Update(jobId, TrassenfinderJobState.Failed, 0, ex.Message);
        }
    }
}
