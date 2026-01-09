using BauFahrplanMonitor.Core.Importer.Helper;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Helpers;

namespace BauFahrplanMonitor.Core.Interfaces;

public interface IFileImporter {
    Task ImportAsync(
        UjBauDbContext                db,
        ImportFileItem                item,
        IProgress<ImportProgressInfo> progress,
        CancellationToken             token);
}