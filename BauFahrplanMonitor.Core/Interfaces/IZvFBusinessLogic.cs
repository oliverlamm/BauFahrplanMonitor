using BauFahrplanMonitor.Core.Importer.Dto.ZvF;

namespace BauFahrplanMonitor.Core.Interfaces;

public interface IZvFBusinessLogic {
    void Apply(ZvFXmlDocumentDto dto, CancellationToken token);
}