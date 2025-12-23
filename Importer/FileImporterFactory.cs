using System;
using BauFahrplanMonitor.Helpers;
using BauFahrplanMonitor.Importer.Interface;
using BauFahrplanMonitor.Interfaces;

namespace BauFahrplanMonitor.Importer;

public sealed class FileImporterFactory(
    NetzfahrplanImporter netzImporter,
    ZvFExportImporter    zvfImporter)
    : IFileImporterFactory {
    public IFileImporter GetImporter(ImporterTyp typ) {
        return typ switch {
            ImporterTyp.Direkt    => netzImporter,
            ImporterTyp.ZvFExport => zvfImporter,
            _ => throw new InvalidOperationException(
                $"Unbekannter ImporterTyp: {typ}")
        };
    }
}