using BauFahrplanMonitor.Core.Dto;

namespace BauFahrplanMonitor.Core.Data.Repositories;

public interface IZugTimelineRepository {
    Task<IReadOnlyList<RawZugTimelineRow>> LoadAsync(
        int               zugNr,
        DateOnly          date,
        CancellationToken ct
    );
}