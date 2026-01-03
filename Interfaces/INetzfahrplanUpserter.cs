using System;
using System.Threading;
using System.Threading.Tasks;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Importer.Dto.Nfpl;
using BauFahrplanMonitor.Importer.Helper;

namespace BauFahrplanMonitor.Interfaces;

public interface INetzfahrplanUpserter {
    Task UpsertAsync(
        UjBauDbContext  db,
        NetzfahrplanDto dto,
        INfplZugCache   zugCache,
        IProgress<UpsertProgressInfo>? progress,
        CancellationToken              token);
}