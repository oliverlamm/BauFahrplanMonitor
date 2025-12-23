using BauFahrplanMonitor.Importer.Xml;

namespace BauFahrplanMonitor.Importer.Interface;

public interface IZvFExportXmlLoader {
    ZvFExport Load(string filePath);
}