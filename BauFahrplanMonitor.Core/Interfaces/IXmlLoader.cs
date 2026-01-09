using System.Xml.Linq;

namespace BauFahrplanMonitor.Core.Interfaces;

public interface IXmlLoader {
    XDocument Load(string filePath);
}

public sealed class XmlLoader : IXmlLoader {
    public XDocument Load(string filePath) {
        // ggf. PreserveWhitespace, SetBaseUri etc. später ergänzen
        return XDocument.Load(filePath, LoadOptions.SetLineInfo);
    }
}