// Core/Interfaces/ITrassenfinderRefreshJob.cs

using BauFahrplanMonitor.Core.Jobs;

namespace BauFahrplanMonitor.Core.Interfaces;

public interface ITrassenfinderRefreshJob {
    Task RefreshInfrastrukturAsync(
        long                                infrastrukturId,
        IProgress<TrassenfinderInfraStatus> progress,
        CancellationToken                   token = default);
}