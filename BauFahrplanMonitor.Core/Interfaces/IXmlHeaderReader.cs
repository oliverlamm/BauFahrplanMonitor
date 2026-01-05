namespace BauFahrplanMonitor.Core.Interfaces;

public interface IXmlHeaderReader<THeader> {
    THeader Read(string filePath);
}
