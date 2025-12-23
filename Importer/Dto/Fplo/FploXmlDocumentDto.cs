using BauFahrplanMonitor.Importer.Dto.Shared;

namespace BauFahrplanMonitor.Importer.Dto.Fplo;

public class FploXmlDocumentDto {
    public SharedHeaderDto  Header   { get; set; } = new();
    public SharedVorgangDto Vorgang  { get; set; } = new();
    public FploDocumentDto  Document { get; set; } = new();
}