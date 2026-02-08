using BauFahrplanMonitor.Core.Data;
using BauFahrplanMonitor.Core.Importer.Dto.Fplo;
using BauFahrplanMonitor.Core.Importer.Helper;

namespace BauFahrplanMonitor.Core.Interfaces;

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