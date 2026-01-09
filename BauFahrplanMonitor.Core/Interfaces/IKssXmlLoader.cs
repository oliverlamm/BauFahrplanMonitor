using BauFahrplanMonitor.Core.Importer.Xml;

namespace BauFahrplanMonitor.Core.Interfaces;

public interface IKssXmlLoader {
    KSSxmlSchnittstelle Load(string filePath);
}