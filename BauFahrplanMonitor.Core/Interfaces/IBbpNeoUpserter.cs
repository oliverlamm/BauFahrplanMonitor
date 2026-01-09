using BauFahrplanMonitor.Core.Importer.Dto.BbpNeo;
using BauFahrplanMonitor.Data;

namespace BauFahrplanMonitor.Core.Interfaces;

public interface IBbpNeoUpserter {
    Task UpsertMassnahmeWithChildrenAsync(
        UjBauDbContext  db,
        BbpNeoMassnahme domain,
        IReadOnlyList<string> warnings,
        Action                onRegelungUpserted,
        Action                onBveUpserted,
        Action                onApsUpserted,
        Action                onIavUpserted,
        CancellationToken     token);
}