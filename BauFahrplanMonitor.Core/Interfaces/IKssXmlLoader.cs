using BauFahrplanMonitor.Importer.Xml;

namespace BauFahrplanMonitor.Interfaces;

public interface IKssXmlLoader {
    KSSxmlSchnittstelle Load(string filePath);
}