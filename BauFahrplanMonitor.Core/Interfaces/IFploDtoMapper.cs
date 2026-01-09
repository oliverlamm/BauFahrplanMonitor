using BauFahrplanMonitor.Core.Importer.Dto.Fplo;
using BauFahrplanMonitor.Core.Importer.Xml;

namespace BauFahrplanMonitor.Core.Interfaces;

public interface IFploDtoMapper {
    FploXmlDocumentDto Map(ZvFExport export, string sourceFile);
}