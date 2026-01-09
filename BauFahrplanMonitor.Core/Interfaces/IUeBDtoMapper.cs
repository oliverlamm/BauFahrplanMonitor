using BauFahrplanMonitor.Core.Importer.Dto.Ueb;
using BauFahrplanMonitor.Core.Importer.Xml;

namespace BauFahrplanMonitor.Core.Interfaces;

public interface IUeBDtoMapper {
    UebXmlDocumentDto Map(ZvFExport export, string sourceFile);
}