using BauFahrplanMonitor.Core.Helpers;

namespace BauFahrplanMonitor.Core.Interfaces;

public interface IFileImporterFactory {
    IFileImporter GetImporter(ImporterTyp typ);
}