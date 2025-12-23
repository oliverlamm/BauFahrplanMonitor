using BauFahrplanMonitor.Importer.Dto.ZvF;
using BauFahrplanMonitor.Importer.Xml;

namespace BauFahrplanMonitor.Importer.Interface;

public interface IZvFDtoMapper {
    ZvFXmlDocumentDto Map(ZvFExport export, string sourceFile);
}