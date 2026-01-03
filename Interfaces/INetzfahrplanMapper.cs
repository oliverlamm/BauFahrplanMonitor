using BauFahrplanMonitor.Importer.Dto.Nfpl;
using BauFahrplanMonitor.Importer.Xml;

namespace BauFahrplanMonitor.Interfaces;

public interface INetzfahrplanMapper {
    NetzfahrplanDto Map(
        KSSxmlSchnittstelle xml,
        string              filePath);
}