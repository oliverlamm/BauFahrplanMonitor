using BauFahrplanMonitor.Importer.Dto.Shared;

namespace BauFahrplanMonitor.Importer.Dto.Ueb;

public class UebXmlDocumentDto {
    public SharedHeaderDto  Header   { get; set; } = new();
    public SharedVorgangDto Vorgang  { get; set; } = new();
    public UebDocumentDto   Document { get; set; } = new();
}