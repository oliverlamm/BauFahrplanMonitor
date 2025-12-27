using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Importer.Dto.BbpNeo;

namespace BauFahrplanMonitor.Importer.Interface;

public interface IBbpNeoUpsertService {
    Task UpsertMassnahmeWithChildrenAsync(
        UjBauDbContext  db,
        BbpNeoMassnahme massnahme,
        IReadOnlyList<string> warnings,
        CancellationToken     token);
}