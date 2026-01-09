using System.Xml.Serialization;
using BauFahrplanMonitor.Core.Interfaces;

namespace BauFahrplanMonitor.Core.Importer.Xml;

public sealed class KssXmlLoader : IKssXmlLoader
{
    private static readonly XmlSerializer Serializer =
        new(typeof(KSSxmlSchnittstelle));

    public KSSxmlSchnittstelle Load(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException(
                "FilePath ist leer.",
                nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException(
                "KSS-Datei nicht gefunden.",
                filePath);

        try {
            using var stream = File.OpenRead(filePath);

            if (Serializer.Deserialize(stream) is not KSSxmlSchnittstelle kss)
                throw new InvalidOperationException(
                    "KSS-XML konnte nicht deserialisiert werden.");

            return kss;
        }
        catch (InvalidOperationException ex) {
            // XmlSerializer wirft diese bei XML-Fehlern
            throw new InvalidOperationException(
                $"Fehler beim Lesen der KSS-XML: {Path.GetFileName(filePath)}",
                ex);
        }
    }
}