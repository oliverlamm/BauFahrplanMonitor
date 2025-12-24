using System;
using System.Threading;
using System.Threading.Tasks;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Importer.Dto.ZvF;
using BauFahrplanMonitor.Importer.Helper;

namespace BauFahrplanMonitor.Importer.Interface;

public interface IZvFUpserter {
    Task<UpsertResult> UpsertAsync(
        UjBauDbContext                 db,
        ZvFXmlDocumentDto              dto,
        IProgress<UpsertProgressInfo>? progress,
        CancellationToken              token);

    
    Task MarkImportCompletedAsync(long zvfDokumentRef, CancellationToken token);
}