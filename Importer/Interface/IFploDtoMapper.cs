using BauFahrplanMonitor.Importer.Dto.Fplo;
using BauFahrplanMonitor.Importer.Xml;

namespace BauFahrplanMonitor.Importer.Interface;

public interface IFploDtoMapper {
    FploXmlDocumentDto Map(ZvFExport export, string sourceFile);
}