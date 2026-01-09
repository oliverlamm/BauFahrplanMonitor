using BauFahrplanMonitor.Core.Importer.Dto.ZvF;
using BauFahrplanMonitor.Core.Importer.Helper;
using BauFahrplanMonitor.Data;

namespace BauFahrplanMonitor.Core.Interfaces;

public interface IZvFUpserter {
    Task<UpsertResult> UpsertAsync(
        UjBauDbContext                 db,
        ZvFXmlDocumentDto              dto,
        IProgress<UpsertProgressInfo>? progress,
        CancellationToken              token);

    
    Task MarkImportCompletedAsync(long zvfDokumentRef, CancellationToken token);
}