using System;
using System.Threading;
using System.Xml;
using BauFahrplanMonitor.Helpers;
using BauFahrplanMonitor.Importer.Dto.BbpNeo;
using NLog;

namespace BauFahrplanMonitor.Importer.Xml;

public static class BbpNeoRawXmlParser {
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
                case "bVEId": dto.BveId                                               = ReadElementText(reader); break;
                case "aktiv": dto.Aktiv                                               = ReadElementText(reader); break;
                case "bVEArtText": dto.ArtText                                        = ReadElementText(reader); break;
                case "bVEBstVonRil100": dto.BstVonRil100                              = ReadElementText(reader); break;
                case "bVEBstBisRil100": dto.BstBisRil100                              = ReadElementText(reader); break;
                case "bVEVzGStrecke": dto.VzGStrecke                                  = ReadElementText(reader); break;
                case "bVEVzGStreckeBis": dto.VzGStreckeBis                            = ReadElementText(reader); break;
                case "bVEGueltigkeit": dto.Gueltigkeit                                = ReadElementText(reader); break;
                case "bVEIAV": dto.Iav                                                = ParseIav(reader, token); break;
                case "bVEAPS": dto.Aps                                                = ParseAps(reader, token); break;
                case "bVEGueltigkeitTagVon": dto.TagVon                               = ReadElementText(reader); break;
                case "bVEGueltigkeitZeitVon": dto.ZeitVon                             = ReadElementText(reader); break;
                case "bVEGueltigkeitTagBis": dto.TagBis                               = ReadElementText(reader); break;
                case "bVEGueltigkeitZeitBis": dto.ZeitBis                             = ReadElementText(reader); break;
                case "bVEGueltigkeitEffektiveVerkehrstage": dto.EffektiveVerkehrstage = ReadElementText(reader); break;
                case "bVEOrtMikroskop": dto.OrtMikroskop                              = ReadElementText(reader); break;
                case "bVEBemerkung": dto.Bemerkung                                    = ReadElementText(reader); break;
                default: SkipElement(reader); break;
            }
        }

        if (reader.NodeType  == XmlNodeType.EndElement &&
            reader.LocalName == startName)
            reader.Read();

        Logger.Debug("RAW BVE:\n{Dump}", dto.Dump());
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

            token.ThrowIfCancellationRequested();

            switch (reader.LocalName) {
                case "Betroffenheit":
                    dto.Betroffenheit = ReadElementText(reader);
                    break;

                case "Beschreibung":
                    dto.Beschreibung = ReadElementText(reader);
                    break;

                case "bVEIAVBetroffenheiten":
                    ParseIavBetroffenheiten(reader, dto, token);
                    break;

                default:
                    SkipElement(reader);
                    break;
            }
        }

        if (reader.NodeType == XmlNodeType.EndElement)
            reader.Read();

        return dto;
    }

    private static void ParseIavBetroffenheiten(
        XmlReader         reader,
        BbpNeoIavRaw      iav,
        CancellationToken token) {

        if (reader.IsEmptyElement) {
            reader.Read();
            return;
        }

        var depth = reader.Depth;
        reader.Read();

        while (reader.MoveToContent() == XmlNodeType.Element &&
               reader.Depth           > depth) {

            token.ThrowIfCancellationRequested();

            if (reader.LocalName == "IAVBetroffenheit") {
                iav.Betroffenheiten.Add(ParseIavBetroffenheit(reader));
            }
            else {
                SkipElement(reader);
            }
        }

        if (reader.NodeType == XmlNodeType.EndElement)
            reader.Read();
    }

    private static BbpNeoIavBetroffenheitRaw ParseIavBetroffenheit(XmlReader reader) {
        var dto = new BbpNeoIavBetroffenheitRaw();

        if (reader.IsEmptyElement) {
            reader.Read();
            return dto;
        }

        var depth = reader.Depth;
        reader.Read();

        while (reader.MoveToContent() == XmlNodeType.Element &&
               reader.Depth           > depth) {

            switch (reader.LocalName) {
                case "VertragNr": dto.VertragNr             = ReadElementText(reader); break;
                case "VertragArt": dto.VertragArt           = ReadElementText(reader); break;
                case "VertragStatus": dto.VertragStatus     = ReadElementText(reader); break;
                case "Kunde": dto.Kunde                     = ReadElementText(reader); break;
                case "Betriebsstelle": dto.Betriebsstelle   = ReadElementText(reader); break;
                case "VzGStrecke": dto.VzGStrecke           = ReadElementText(reader); break;
                case "Anschlussgrenze": dto.Anschlussgrenze = ReadElementText(reader); break;
                case "Oberleitung": dto.Oberleitung         = ReadElementText(reader); break;
                case "OberleitungAus": dto.OberleitungAus   = ReadElementText(reader); break;
                case "EinschraenkungBedienbarkeitIA":
                    dto.EinschraenkungBedienbarkeitIA = ReadElementText(reader);
                    break;
                case "Kommentar": dto.Kommentar = ReadElementText(reader); break;
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
        reader.Read(); // rein in <bVEAPS>

        while (reader.MoveToContent() == XmlNodeType.Element &&
               reader.Depth           > depth) {

            token.ThrowIfCancellationRequested();

            switch (reader.LocalName) {

                case "Betroffenheit":
                    dto.Betroffenheit = ReadElementText(reader);
                    break;

                case "Beschreibung":
                    dto.Beschreibung = ReadElementText(reader);
                    break;

                case "FreiVonFahrzeugen":
                    dto.FreiVonFahrzeugen = ReadElementText(reader);
                    break;

                case "bVEAPSBetroffenheiten":
                    ParseApsBetroffenheiten(reader, dto, token);
                    break;

                default:
                    SkipElement(reader);
                    break;
            }
        }

        if (reader.NodeType == XmlNodeType.EndElement)
            reader.Read(); // </bVEAPS>

        return dto;
    }

    private static void ParseApsBetroffenheiten(
        XmlReader         reader,
        BbpNeoApsRaw      aps,
        CancellationToken token) {

        if (reader.IsEmptyElement) {
            reader.Read();
            return;
        }

        var depth = reader.Depth;
        reader.Read(); // rein in <bVEAPSBetroffenheiten>

        while (reader.MoveToContent() == XmlNodeType.Element &&
               reader.Depth           > depth) {

            token.ThrowIfCancellationRequested();

            if (reader.LocalName.Equals("APSBetroffenheit", StringComparison.OrdinalIgnoreCase)) {
                aps.Betroffenheiten.Add(ParseApsBetroffenheit(reader, token));
            }
            else {
                SkipElement(reader);
            }
        }

        if (reader.NodeType == XmlNodeType.EndElement)
            reader.Read(); // </bVEAPSBetroffenheiten>
    }

    private static BbpNeoApsBetroffenheitRaw ParseApsBetroffenheit(
        XmlReader         reader,
        CancellationToken token) {

        var dto = new BbpNeoApsBetroffenheitRaw();

        if (reader.IsEmptyElement) {
            reader.Read();
            return dto;
        }

        var depth = reader.Depth;
        reader.Read(); // rein in <APSBetroffenheit>

        while (reader.MoveToContent() == XmlNodeType.Element &&
               reader.Depth           > depth) {

            token.ThrowIfCancellationRequested();

            switch (reader.LocalName) {

                case "UUID":
                    dto.Uuid = ReadElementText(reader);
                    break;

                case "DS100":
                    dto.Ds100 = ReadElementText(reader);
                    break;

                case "GleisNr":
                    dto.GleisNr = ReadElementText(reader);
                    break;

                case "PrimaereKategorie":
                    dto.PrimaereKategorie = ReadElementText(reader);
                    break;

                case "SekundaereKategorie":
                    dto.SekundaereKategorie = ReadElementText(reader);
                    break;

                case "Oberleitung":
                    dto.Oberleitung = ReadElementText(reader);
                    break;

                case "OberleitungAus":
                    dto.OberleitungAus = ReadElementText(reader);
                    break;

                case "TechnischerPlatz":
                    dto.TechnischerPlatz = ReadElementText(reader);
                    break;

                case "ArtDerAnbindung":
                    dto.ArtDerAnbindung = ReadElementText(reader);
                    break;

                case "EinschraenkungBefahrbarkeitSE":
                    dto.EinschraenkungBefahrbarkeitSE = ReadElementText(reader);
                    break;

                case "Kommentar":
                    dto.Kommentar = ReadElementText(reader);
                    break;

                case "AbFahrplanjahr":
                    dto.AbFahrplanjahr = ReadElementText(reader);
                    break;

                case "moeglicheZA":
                    ParseMoeglicheZa(reader, dto, token);
                    break;

                default:
                    SkipElement(reader);
                    break;
            }
        }

        if (reader.NodeType == XmlNodeType.EndElement)
            reader.Read(); // </APSBetroffenheit>

        return dto;
    }

    private static void ParseMoeglicheZa(
        XmlReader                 reader,
        BbpNeoApsBetroffenheitRaw aps,
        CancellationToken         token) {

        if (reader.IsEmptyElement) {
            reader.Read();
            return;
        }

        var depth = reader.Depth;
        reader.Read(); // rein in <moeglicheZA>

        while (reader.MoveToContent() == XmlNodeType.Element &&
               reader.Depth           > depth) {

            token.ThrowIfCancellationRequested();

            if (reader.LocalName.Equals("za", StringComparison.OrdinalIgnoreCase)) {
                aps.MoeglicheZa!.Add(ParseZa(reader, token));
            }
            else {
                SkipElement(reader);
            }
        }

        if (reader.NodeType == XmlNodeType.EndElement)
            reader.Read(); // </moeglicheZA>
    }

    private static BbpNeoZaRaw ParseZa(
        XmlReader         reader,
        CancellationToken token) {

        var dto = new BbpNeoZaRaw();

        if (reader.IsEmptyElement) {
            reader.Read();
            return dto;
        }

        var depth = reader.Depth;
        reader.Read(); // rein in <za>

        while (reader.MoveToContent() == XmlNodeType.Element &&
               reader.Depth           > depth) {

            token.ThrowIfCancellationRequested();

            switch (reader.LocalName) {

                case "uuidZa":
                    dto.UuidZa = ReadElementText(reader);
                    break;

                case "typZa":
                    dto.TypZa = ReadElementText(reader);
                    break;

                case "objektnummer":
                    dto.Objektnummer = ReadElementText(reader);
                    break;

                default:
                    SkipElement(reader);
                    break;
            }
        }

        if (reader.NodeType == XmlNodeType.EndElement)
            reader.Read(); // </za>

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