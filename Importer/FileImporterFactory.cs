using System;
using BauFahrplanMonitor.Helpers;
using BauFahrplanMonitor.Importer.Interface;
using BauFahrplanMonitor.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BauFahrplanMonitor.Importer;

public sealed class FileImporterFactory(IServiceProvider provider)
    : IFileImporterFactory {
    public IFileImporter GetImporter(ImporterTyp typ) {
        return typ switch {
            ImporterTyp.Direkt =>
                provider.GetRequiredService<NetzfahrplanImporter>(),

            ImporterTyp.ZvFExport =>
                provider.GetRequiredService<ZvFExportImporter>(),

            _ => throw new InvalidOperationException(
                $"Unbekannter ImporterTyp: {typ}")
        };
    }
}