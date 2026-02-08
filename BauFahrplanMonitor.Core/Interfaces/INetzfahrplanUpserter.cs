using BauFahrplanMonitor.Core.Data;
using BauFahrplanMonitor.Core.Importer.Dto.Nfpl;
using BauFahrplanMonitor.Core.Importer.Helper;

namespace BauFahrplanMonitor.Core.Interfaces;

public interface INetzfahrplanUpserter {
    Task UpsertAsync(
        UjBauDbContext  db,
        NetzfahrplanDto dto,
        INfplZugCache   zugCache,
        IProgress<UpsertProgressInfo>? progress,
        CancellationToken              token);
}