using BauFahrplanMonitor.Core.Importer.Xml;

namespace BauFahrplanMonitor.Core.Interfaces;

public interface IZvFExportXmlLoader {
    ZvFExport Load(string filePath);
}