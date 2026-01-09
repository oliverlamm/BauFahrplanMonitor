using BauFahrplanMonitor.Core.Importer.Dto.ZvF;
using BauFahrplanMonitor.Core.Importer.Xml;

namespace BauFahrplanMonitor.Core.Interfaces;

public interface IZvFDtoMapper {
    ZvFXmlDocumentDto Map(ZvFExport export, string sourceFile);
}