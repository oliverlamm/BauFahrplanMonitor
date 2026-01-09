using BauFahrplanMonitor.Core.Interfaces;
using BauFahrplanMonitor.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace BauFahrplanMonitor.Core.Importer.Helper;

public sealed class FileImporterFactory(IServiceProvider provider)
    : IFileImporterFactory {
    public IFileImporter GetImporter(ImporterTyp typ) {
        return typ switch {
            ImporterTyp.Direkt =>
                provider.GetRequiredService<NetzfahrplanImporter>(),

            ImporterTyp.ZvFExport =>
                provider.GetRequiredService<ZvFExportImporter>(),

            ImporterTyp.BBPNeo =>
                provider.GetRequiredService<BbpNeoImporter>(),

            //ImporterTyp.OsbBob =>
            //    provider.GetRequiredService<OsbBobImporter>(),

            _ => throw new InvalidOperationException(
                $"Unbekannter ImporterTyp: {typ}")
        };
    }
}