using System;
using System.Threading;
using System.Xml;
using BauFahrplanMonitor.Importer.Dto.BbpNeo;

namespace BauFahrplanMonitor.Importer.Xml;

public static class BbpNeoRawXmlParser {

    // =====================================================================
    // ENTRY: <BBPMassnahme>
    // =====================================================================
    public static BbpNeoMassnahmeRaw ParseMassnahme(
        XmlReader         reader,
        int               taskId,
        CancellationToken token) {

        token.ThrowIfCancellationRequested();

        MoveToElement(reader);
        EnsureElement(reader, "BBPMassnahme");

        var dto = new BbpNeoMassnahmeRaw();

        if (reader.IsEmptyElement) {
            reader.Read();
            return dto;
        }

        var depth = reader.Depth;
        reader.Read(); // INS <BBPMassnahme>

        while (reader.MoveToContent() == XmlNodeType.Element &&
               reader.Depth           > depth) {

            token.ThrowIfCancellationRequested();

            switch (reader.LocalName) {

                case "MasID": dto.MasId = ReadElementText(reader); break;
                case "aktiv": dto.Aktiv = ReadElementText(reader); break;

                case "Vorhaben": dto.Vorhaben = ReadElementText(reader); break;

                case "RegionID": dto.RegionId = ReadElementText(reader); break;
                case "Region": dto.Region     = ReadElementText(reader); break;

                case "Arbeiten": dto.Arbeiten             = ReadElementText(reader); break;
                case "ArtDerArbeiten": dto.ArtDerArbeiten = ReadElementText(reader); break;

                case "VzGStrecke": dto.VzGStrecke       = ReadElementText(reader); break;
                case "VzGStreckeBis": dto.VzGStreckeBis = ReadElementText(reader); break;

                case "MasBstVonRil100": dto.MasBstVonRil100 = ReadElementText(reader); break;
                case "MasBstBisRil100": dto.MasBstBisRil100 = ReadElementText(reader); break;

                case "MasKmVon": dto.MasKmVon = ReadElementText(reader); break;
                case "MasKmBis": dto.MasKmBis = ReadElementText(reader); break;

                case "MasBeginn": dto.MasBeginn = ReadElementText(reader); break;
                case "MasEnde": dto.MasEnde     = ReadElementText(reader); break;

                case "Genehmigung": dto.Genehmigung           = ReadElementText(reader); break;
                case "Anmeldung": dto.Anmeldung               = ReadElementText(reader); break;
                case "DatumAuftragBBZR": dto.DatumAuftragBBZR = ReadElementText(reader); break;

                case "Regelungen":
                    ParseRegelungenContainer(reader, dto, token);
                    break;

                default:
                    SkipElement(reader);
                    break;
            }
        }

        if (reader.NodeType  == XmlNodeType.EndElement &&
            reader.LocalName == "BBPMassnahme")
            reader.Read();

        return dto;
    }

    // =====================================================================
    // REGELUNGEN
    // =====================================================================
    private static void ParseRegelungenContainer(
        XmlReader          reader,
        BbpNeoMassnahmeRaw massnahme,
        CancellationToken  token) {

        EnsureElement(reader, "Regelungen");

        if (reader.IsEmptyElement) {
            reader.Read();
            return;
        }

        var depth = reader.Depth;
        reader.Read();

        while (reader.MoveToContent() == XmlNodeType.Element &&
               reader.Depth           > depth) {

            token.ThrowIfCancellationRequested();

            if (reader.LocalName == "Regelung") {
                var r = ParseRegelung(reader, token);
                massnahme.Regelungen.Add(r);
            }
            else {
                SkipElement(reader);
            }
        }

        if (reader.NodeType  == XmlNodeType.EndElement &&
            reader.LocalName == "Regelungen")
            reader.Read();
    }

    private static BbpNeoRegelungRaw ParseRegelung(
        XmlReader         reader,
        CancellationToken token) {

        EnsureElement(reader, "Regelung");
        var dto = new BbpNeoRegelungRaw();

        if (reader.IsEmptyElement) {
            reader.Read();
            return dto;
        }

        var depth = reader.Depth;
        reader.Read();

        while (reader.MoveToContent() == XmlNodeType.Element &&
               reader.Depth           > depth) {

            token.ThrowIfCancellationRequested();

            switch (reader.LocalName) {

                case "RegID": dto.RegId = ReadElementText(reader); break;
                case "aktiv": dto.Aktiv = ReadElementText(reader); break;

                case "Beginn": dto.Beginn     = ReadElementText(reader); break;
                case "Ende": dto.Ende         = ReadElementText(reader); break;
                case "Zeitraum": dto.Zeitraum = ReadElementText(reader); break;

                case "BstVonRil100": dto.BstVonRil100 = ReadElementText(reader); break;
                case "BstBisRil100": dto.BstBisRil100 = ReadElementText(reader); break;

                case "VzGStrecke": dto.VzGStrecke       = ReadElementText(reader); break;
                case "VzGStreckeBis": dto.VzGStreckeBis = ReadElementText(reader); break;

                case "BplArtText": dto.BplArtText           = ReadElementText(reader); break;
                case "BplRegelungKurz": dto.BplRegelungKurz = ReadElementText(reader); break;
                case "BplRegelungLang": dto.BplRegelungLang = ReadElementText(reader); break;

                case "durchgehend": dto.Durchgehend   = ReadElementText(reader); break;
                case "schichtweise": dto.Schichtweise = ReadElementText(reader); break;

                case "BemerkungenBpl": dto.BemerkungenBpl = ReadElementText(reader); break;
                case "BemerkungenFpl": dto.BemerkungenFpl = ReadElementText(reader); break;

                case "Bven":
                case "bVEn":
                    ParseBveContainer(reader, dto, token);
                    break;

                default:
                    SkipElement(reader);
                    break;
            }
        }

        if (reader.NodeType  == XmlNodeType.EndElement &&
            reader.LocalName == "Regelung")
            reader.Read();

        return dto;
    }

    // =====================================================================
    // BVE / IAV / APS
    // =====================================================================
    // (gleiches Muster – vollständig synchron)

    // --- BVE ---
    private static BbpNeoBveRaw ParseBve(XmlReader reader, CancellationToken token) {
        var startName = reader.LocalName;
        var dto       = new BbpNeoBveRaw();

        if (reader.IsEmptyElement) {
            reader.Read();
            return dto;
        }

        var depth = reader.Depth;
        reader.Read();

        while (reader.MoveToContent() == XmlNodeType.Element &&
               reader.Depth           > depth) {

            token.ThrowIfCancellationRequested();

            switch (reader.LocalName) {
                case "bVEId": dto.BveId                    = ReadElementText(reader); break;
                case "aktiv": dto.Aktiv                    = ReadElementText(reader); break;
                case "bVEArtText": dto.ArtText             = ReadElementText(reader); break;
                case "bVEBstVonRil100": dto.BstVonRil100   = ReadElementText(reader); break;
                case "bVEBstBisRil100": dto.BstBisRil100   = ReadElementText(reader); break;
                case "bVEVzGStrecke": dto.VzGStrecke       = ReadElementText(reader); break;
                case "bVEVzGStreckeBis": dto.VzGStreckeBis = ReadElementText(reader); break;
                case "bVEGueltigkeit": dto.Gueltigkeit     = ReadElementText(reader); break;
                case "bVEIAV": dto.Iav                     = ParseIav(reader, token); break;
                case "bVEAPS": dto.Aps                     = ParseAps(reader, token); break;
                default: SkipElement(reader); break;
            }
        }

        if (reader.NodeType  == XmlNodeType.EndElement &&
            reader.LocalName == startName)
            reader.Read();

        return dto;
    }

    private static void ParseBveContainer(
        XmlReader         reader,
        BbpNeoRegelungRaw regelung,
        CancellationToken token) {

        var depth = reader.Depth;
        reader.Read();

        while (reader.MoveToContent() == XmlNodeType.Element &&
               reader.Depth           > depth) {

            if (reader.LocalName.Equals("bVE", StringComparison.OrdinalIgnoreCase) ||
                reader.LocalName.Equals("BVE", StringComparison.OrdinalIgnoreCase)) {

                regelung.Bven.Add(ParseBve(reader, token));
            }
            else {
                SkipElement(reader);
            }
        }

        if (reader.NodeType == XmlNodeType.EndElement)
            reader.Read();
    }

    // --- IAV ---
    private static BbpNeoIavRaw ParseIav(XmlReader reader, CancellationToken token) {
        var dto = new BbpNeoIavRaw();

        if (reader.IsEmptyElement) {
            reader.Read();
            return dto;
        }

        var depth = reader.Depth;
        reader.Read();

        while (reader.MoveToContent() == XmlNodeType.Element &&
               reader.Depth           > depth) {

            switch (reader.LocalName) {
                case "Betroffenheit": dto.Betroffenheit = ReadElementText(reader); break;
                case "Beschreibung": dto.Beschreibung   = ReadElementText(reader); break;
                default: SkipElement(reader); break;
            }
        }

        if (reader.NodeType == XmlNodeType.EndElement)
            reader.Read();

        return dto;
    }

    // --- APS ---
    private static BbpNeoApsRaw ParseAps(XmlReader reader, CancellationToken token) {
        var dto = new BbpNeoApsRaw();

        if (reader.IsEmptyElement) {
            reader.Read();
            return dto;
        }

        var depth = reader.Depth;
        reader.Read();

        while (reader.MoveToContent() == XmlNodeType.Element &&
               reader.Depth           > depth) {

            switch (reader.LocalName) {
                case "Betroffenheit": dto.Betroffenheit = ReadElementText(reader); break;
                case "Beschreibung": dto.Beschreibung   = ReadElementText(reader); break;
                default: SkipElement(reader); break;
            }
        }

        if (reader.NodeType == XmlNodeType.EndElement)
            reader.Read();

        return dto;
    }

    // =====================================================================
    // Helpers (synchron)
    // =====================================================================
    private static void MoveToElement(XmlReader reader) {
        if (reader.ReadState == ReadState.Initial)
            reader.Read();
        reader.MoveToContent();
    }

    private static void EnsureElement(XmlReader reader, string localName) {
        if (reader.NodeType != XmlNodeType.Element ||
            !reader.LocalName.Equals(localName, StringComparison.OrdinalIgnoreCase))
            throw new XmlException($"Expected <{localName}> but was <{reader.LocalName}>");
    }

    private static string? ReadElementText(XmlReader reader) {
        if (reader.IsEmptyElement) {
            reader.Read();
            return null;
        }

        var value = reader.ReadElementContentAsString();
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static void SkipElement(XmlReader reader) {
        if (reader.IsEmptyElement) {
            reader.Read();
            return;
        }

        var depth = reader.Depth;
        reader.Read();

        while (!reader.EOF && reader.Depth > depth)
            reader.Read();

        if (reader.NodeType == XmlNodeType.EndElement)
            reader.Read();
    }
}