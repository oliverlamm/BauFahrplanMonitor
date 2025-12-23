using BauFahrplanMonitor.Helpers;
using BauFahrplanMonitor.Interfaces;

namespace BauFahrplanMonitor.Importer.Interface;

public interface IFileImporterFactory {
    IFileImporter GetImporter(ImporterTyp typ);
}