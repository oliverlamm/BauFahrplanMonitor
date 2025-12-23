using BauFahrplanMonitor.Importer.Dto.Shared;

namespace BauFahrplanMonitor.Importer.Dto.ZvF;

public class ZvFXmlDocumentDto {
    public SharedHeaderDto Header   { get; set; } = new();
    public ZvFVorgangDto   Vorgang  { get; set; } = new();
    public ZvFDocumentDto  Document { get; set; } = new();
}