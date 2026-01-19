using BauFahrplanMonitor.Core.Helpers;
using BauFahrplanMonitor.Core.Importer.Helper;
using BauFahrplanMonitor.Data;

namespace BauFahrplanMonitor.Core.Interfaces;

public interface IFileImporter {
    Task<ImportFileOutcome> ImportAsync(
        UjBauDbContext                db,
        ImportFileItem                item,
        IProgress<ImportProgressInfo> progress,
        CancellationToken             token);
}