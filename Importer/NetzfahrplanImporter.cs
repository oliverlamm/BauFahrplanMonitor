using System;
using System.Threading;
using System.Threading.Tasks;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Helpers;
using BauFahrplanMonitor.Importer.Helper;
using BauFahrplanMonitor.Interfaces;

namespace BauFahrplanMonitor.Importer;

public class NetzfahrplanImporter : IFileImporter {
    public Task ImportAsync(ImportFileItem item, CancellationToken token) {
        // Netzfahrplan-Logik
        return Task.CompletedTask;
    }

    public Task ImportAsync(UjBauDbContext db, ImportFileItem item, IProgress<ImportProgressInfo> progress, CancellationToken token) {
        throw new NotImplementedException();
    }
}