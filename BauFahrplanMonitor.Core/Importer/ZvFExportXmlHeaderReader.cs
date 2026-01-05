using System.Xml;
using BauFahrplanMonitor.Core.Interfaces;

namespace BauFahrplanMonitor.Core.Importer;

public sealed class ZvFExportXmlHeaderReader
    : IXmlHeaderReader<ZvFExportHeader> {

    public ZvFExportHeader Read(string filePath) {
        using var stream = File.OpenRead(filePath);
        using var reader = XmlReader.Create(stream, new XmlReaderSettings {
            IgnoreComments   = true,
            IgnoreWhitespace = true
        });

        while (reader.Read()) {
            if (reader is {
                    NodeType: XmlNodeType.Element,
                    Name: "Document"
                }) {

                var id     = reader.GetAttribute("id");
                var region = reader.GetAttribute("region");

                return new ZvFExportHeader {
                    DocumentId = id     ?? string.Empty,
                    Region     = region ?? string.Empty
                };
            }
        }

        throw new InvalidOperationException(
            $"Kein gültiger ZvF-Header gefunden: {filePath}");
    }
}