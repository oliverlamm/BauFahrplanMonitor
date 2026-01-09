using System.Xml;
using BauFahrplanMonitor.Core.Importer.Dto.BbpNeo;
using BauFahrplanMonitor.Core.Interfaces;
using NLog;

namespace BauFahrplanMonitor.Core.Importer.Xml;

public sealed class BbpNeoXmlStreamingLoader : IBbpNeoXmlStreamingLoader {

    private static readonly Logger Logger =
        LogManager.GetCurrentClassLogger();

    public async Task StreamAsync(
        string                                            filePath,
        Func<BbpNeoMassnahmeRaw, CancellationToken, Task> onMassnahme,
        CancellationToken                                 token) {

        Logger.Info($"[BBPNeo.Stream] Starte Streaming-Import: {filePath}");

        var settings = new XmlReaderSettings {
            IgnoreComments   = true,
            IgnoreWhitespace = true,
            DtdProcessing    = DtdProcessing.Ignore,
            CloseInput       = true
        };

        await using var stream = File.OpenRead(filePath);
        using var       reader = XmlReader.Create(stream, settings);

        reader.MoveToContent();

        if (!reader.ReadToFollowing("Daten")) {
            throw new InvalidOperationException(
                "BBPNeo-Datei enthält keinen <Daten>-Block.");
        }

        Logger.Debug("[BBPNeo.Stream] <Daten>-Block gefunden");

        // 1) bis zur ersten BBPMassnahme
        while (reader.Read()) {
            token.ThrowIfCancellationRequested();

            if (reader.NodeType  == XmlNodeType.Element &&
                reader.LocalName == "BBPMassnahme")
                break;
        }

        // 2) Maßnahmen sequenziell
        while (!reader.EOF                             &&
               reader.NodeType  == XmlNodeType.Element &&
               reader.LocalName == "BBPMassnahme") {

            token.ThrowIfCancellationRequested();
            
            Logger.Info(
                "[BBPNeo.Stream] ▶ ENTER Massnahme @Pos: Node={0}, Name={1}, Depth={2}",
                reader.NodeType,
                reader.LocalName,
                reader.Depth);

            var raw = BbpNeoRawXmlParser.ParseMassnahme(
                reader,
                taskId: 0,
                token);

            Logger.Info(
                "[BBPNeo.Stream] ✔ PARSED Massnahme MasNr={0} | Reader now: Node={1}, Name={2}, Depth={3}",
                raw?.MasId ?? "<null>",
                reader.NodeType,
                reader.LocalName,
                reader.Depth);

            Logger.Info(
                "[BBPNeo.Stream] ▶ CALLBACK start MasNr={0}",
                raw?.MasId ?? "<null>");

            await onMassnahme(raw, token);

            Logger.Info(
                "[BBPNeo.Stream] ✔ CALLBACK end MasNr={0}",
                raw?.MasId ?? "<null>");

            // Vorspulen
            while (!reader.EOF &&
                   (reader.NodeType  != XmlNodeType.Element ||
                    reader.LocalName != "BBPMassnahme"))
            {
                reader.Read();
            }

            Logger.Info(
                "[BBPNeo.Stream] ▶ NEXT Reader @Pos: Node={0}, Name={1}, Depth={2}, EOF={3}",
                reader.NodeType,
                reader.LocalName,
                reader.Depth,
                reader.EOF);
        }
    }
}