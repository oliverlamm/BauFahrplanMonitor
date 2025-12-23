using System;
using System.Threading;
using System.Threading.Tasks;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Helpers;
using BauFahrplanMonitor.Importer.Helper;

namespace BauFahrplanMonitor.Interfaces;

public interface IFileImporter {
    Task ImportAsync(
        UjBauDbContext                db,
        ImportFileItem                item,
        IProgress<ImportProgressInfo> progress,
        CancellationToken             token);
}