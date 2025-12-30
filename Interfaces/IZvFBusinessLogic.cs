using System.Threading;
using BauFahrplanMonitor.Importer.Dto.ZvF;

namespace BauFahrplanMonitor.Interfaces;

public interface IZvFBusinessLogic {
    void Apply(ZvFXmlDocumentDto dto, CancellationToken token);
}