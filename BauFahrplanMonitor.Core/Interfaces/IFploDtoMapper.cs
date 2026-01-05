using BauFahrplanMonitor.Importer.Dto.Fplo;
using BauFahrplanMonitor.Importer.Xml;

namespace BauFahrplanMonitor.Interfaces;

public interface IFploDtoMapper {
    FploXmlDocumentDto Map(ZvFExport export, string sourceFile);
}