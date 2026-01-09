using BauFahrplanMonitor.Core.Importer.Dto.Ueb;
using BauFahrplanMonitor.Core.Importer.Helper;
using BauFahrplanMonitor.Data;

namespace BauFahrplanMonitor.Core.Interfaces;

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