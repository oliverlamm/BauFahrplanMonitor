using System.Xml.Serialization;
using BauFahrplanMonitor.Core.Interfaces;

namespace BauFahrplanMonitor.Core.Importer.Xml;

public sealed class ZvFExportXmlLoader : IZvFExportXmlLoader {
    private static readonly XmlSerializer Serializer =
        new(typeof(ZvFExport));

    public ZvFExport Load(string filePath) {
        using var fs = File.OpenRead(filePath);
        return Serializer.Deserialize(fs) is not ZvFExport export
            ? throw new InvalidOperationException("ZvFExport konnte nicht geladen werden.")
            : export;
    }
}