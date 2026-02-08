using System.Collections.Concurrent;
using BauFahrplanMonitor.Api.Dto;

namespace BauFahrplanMonitor.Api.Jobs;

public sealed class TrassenfinderJobStore {
    private readonly ConcurrentDictionary<string, TrassenfinderJobStatusDto> _jobs = new();

    public TrassenfinderJobStatusDto Create() {
        var job = new TrassenfinderJobStatusDto {
            JobId    = Guid.NewGuid().ToString(),
            State    = TrassenfinderJobState.Pending,
            Progress = 0,
            Message  = "Warteschlange"
        };

        this._jobs[job.JobId] = job;
        return job;
    }

    public bool TryGet(string jobId, out TrassenfinderJobStatusDto job)
        => this._jobs.TryGetValue(jobId, out job!);

    public void Update(
        string                jobId,
        TrassenfinderJobState state,
        int                   progress,
        string?               message = null) {

        if (!this._jobs.ContainsKey(jobId))
            return;

        this._jobs[jobId] = new TrassenfinderJobStatusDto {
            JobId    = jobId,
            State    = state,
            Progress = progress,
            Message  = message
        };
    }
}