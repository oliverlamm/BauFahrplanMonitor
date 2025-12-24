using BauFahrplanMonitor.Importer.Dto.Ueb;
using BauFahrplanMonitor.Importer.Xml;

namespace BauFahrplanMonitor.Importer.Interface;

public interface IUeBDtoMapper {
    UebXmlDocumentDto Map(ZvFExport export, string sourceFile);
}