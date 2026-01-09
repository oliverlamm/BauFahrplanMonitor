using BauFahrplanMonitor.Core.Importer.Dto.Nfpl;
using BauFahrplanMonitor.Core.Importer.Xml;

namespace BauFahrplanMonitor.Core.Interfaces;

public interface INetzfahrplanMapper {
    NetzfahrplanDto Map(
        KSSxmlSchnittstelle xml,
        string              filePath);
}