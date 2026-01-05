using BauFahrplanMonitor.Helpers;

namespace BauFahrplanMonitor.Interfaces;

public interface IFileImporterFactory {
    IFileImporter GetImporter(ImporterTyp typ);
}