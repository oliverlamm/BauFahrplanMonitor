using BauFahrplanMonitor.Importer.Dto.ZvF;
using BauFahrplanMonitor.Importer.Xml;

namespace BauFahrplanMonitor.Interfaces;

public interface IZvFDtoMapper {
    ZvFXmlDocumentDto Map(ZvFExport export, string sourceFile);
}