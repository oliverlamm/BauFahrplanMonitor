using BauFahrplanMonitor.Importer.Xml;

namespace BauFahrplanMonitor.Interfaces;

public interface IZvFExportXmlLoader {
    ZvFExport Load(string filePath);
}