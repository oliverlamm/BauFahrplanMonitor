using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Importer.Dto.BbpNeo;

namespace BauFahrplanMonitor.Interfaces;

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