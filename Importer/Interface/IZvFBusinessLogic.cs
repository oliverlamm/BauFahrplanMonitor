using System.Threading;
using BauFahrplanMonitor.Importer.Dto.ZvF;

namespace BauFahrplanMonitor.Importer.Interface;

public interface IZvFBusinessLogic {
    void Apply(ZvFXmlDocumentDto dto, CancellationToken token);
}