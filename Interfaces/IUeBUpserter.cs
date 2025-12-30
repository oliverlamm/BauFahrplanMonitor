using System;
using System.Threading;
using System.Threading.Tasks;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Importer.Dto.Ueb;
using BauFahrplanMonitor.Importer.Helper;

namespace BauFahrplanMonitor.Interfaces;

public interface IUeBUpserter {
    Task<UpsertResult> UpsertAsync(
        UjBauDbContext                db,
        UebXmlDocumentDto             dto,
        IProgress<UpsertProgressInfo> progress,
        CancellationToken             token);

    Task MarkImportCompletedAsync(
        long              dokumentRef,
        CancellationToken token);
}