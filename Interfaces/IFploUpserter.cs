using System;
using System.Threading;
using System.Threading.Tasks;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Importer.Dto.Fplo;
using BauFahrplanMonitor.Importer.Helper;

namespace BauFahrplanMonitor.Interfaces;

public interface IFploUpserter {
    Task<UpsertResult> UpsertAsync(
        UjBauDbContext                db,
        FploXmlDocumentDto             dto,
        IProgress<UpsertProgressInfo> progress,
        CancellationToken             token);

    Task MarkImportCompletedAsync(
        long              dokumentRef,
        CancellationToken token);
}