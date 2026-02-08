using BauFahrplanMonitor.Core.Data;
using BauFahrplanMonitor.Core.Helpers;
using BauFahrplanMonitor.Core.Importer.Helper;

namespace BauFahrplanMonitor.Core.Interfaces;

public interface IFileImporter {
    Task<ImportFileOutcome> ImportAsync(
        UjBauDbContext                db,
        ImportFileItem                item,
        IProgress<ImportProgressInfo> progress,
        CancellationToken             token);
}