using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BauFahrplanMonitor.Importer.Dto.Fplo;
using BauFahrplanMonitor.Importer.Dto.Shared;
using BauFahrplanMonitor.Importer.Helper;
using BauFahrplanMonitor.Importer.Interface;
using BauFahrplanMonitor.Importer.Xml;
using NLog;

namespace BauFahrplanMonitor.Importer.Mapper;

public sealed class FploDtoMapper : IFploDtoMapper {
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public FploXmlDocumentDto Map(ZvFExport export, string sourceFile) {
        if (export.Header == null)
            throw new InvalidOperationException("ZvFExport.Header fehlt");

        if (export.Baumassnahmen?.Baumassnahme == null)
            throw new InvalidOperationException("Keine Baumassnahme vorhanden");

        var bm = export.Baumassnahmen.Baumassnahme;

        var dto = new FploXmlDocumentDto();

        // -------------------------------
        // Header (Shared)
        // -------------------------------
        dto.Header.FileName        = export.Header.Filename;
        dto.Header.Timestamp       = export.Header.Timestamp;
        dto.Header.Timestamp       = export.Header.Timestamp;
        dto.Header.FileName        = export.Header.Filename;
        dto.Header.SenderName      = export.Header.Sender?.Name      ?? "";
        dto.Header.SenderVorname   = export.Header.Sender?.Vorname   ?? "";
        dto.Header.SenderMail      = export.Header.Sender?.Email     ?? "";
        dto.Header.SenderTelefon   = export.Header.Sender?.Telefon   ?? "";
        dto.Header.SenderAbteilung = export.Header.Sender?.Abteilung ?? "";
        dto.Header.SenderAdresse   = export.Header.Sender?.Strasse   ?? "";
        dto.Header.SenderStadt     = export.Header.Sender?.Ort       ?? "";
        dto.Header.SenderPlz       = int.TryParse(export.Header.Sender?.Plz, out var plz) ? plz : 0;

        if (export.Header.Empfaengerliste is { Length: > 0 })
            foreach (var e in export.Header.Empfaengerliste)
                dto.Header.Empfaenger.Add(e);

        // -------------------------------
        // Dokument (Meta)
        // -------------------------------
        var fileName = Path.GetFileName(sourceFile);
        dto.Document.Dateiname = fileName;
        dto.Document.Region =
            bm.Fplonr?
                .FirstOrDefault(n => n.Beteiligt == 1)?
                .Value?
                .Trim()
            ?? string.Empty;
        dto.Document.MasterRegion    = bm.Masterniederlassung ?? "";
        dto.Document.ExportTimestamp = export.Header.Timestamp;
        dto.Document.Version.Major   = bm.Version?.Major ?? 0;
        dto.Document.Version.Minor   = bm.Version?.Minor ?? 0;
        dto.Document.Version.Sub     = bm.Version?.Sub   ?? 0;
        dto.Document.Version.VersionNumeric = dto.Document.Version.Major * 1000 +
                                              dto.Document.Version.Minor * 100  +
                                              dto.Document.Version.Sub;

        dto.Document.GueltigAb = bm.GueltigkeitFplo?.Beginn != null
            ? DateOnly.FromDateTime(bm.GueltigkeitFplo.Beginn)
            : null;
        dto.Document.GueltigBis = bm.GueltigkeitFplo?.Ende != null
            ? DateOnly.FromDateTime(bm.GueltigkeitFplo.Ende)
            : null;
        dto.Document.AllgemeinText = string.Join("\n", bm.Allgregelungen ?? []);

        MapDateiFlags(fileName, dto.Document);

        // -------------------------------
        // Vorgang
        // -------------------------------
        dto.Vorgang.MasterFplo = bm.MasterFplo;

        // -------------------------------
        // Streckenabschnitte → SharedStreckeDto + JSON
        // -------------------------------
        MapStreckenabschnitte(bm, dto);

        // -------------------------------
        // Züge
        // -------------------------------
        MapZuege(bm, dto);

        // -------------------------------
        // SEV
        // -------------------------------
        MapSev(bm, dto);

        // -------------------------------
        // Haltausfall
        // -------------------------------
        MapHaltausfall(bm, dto);

        // -------------------------------
        // Zurückgehalten
        // -------------------------------
        MapZurueckgehalten(bm, dto);

        // -------------------------------
        // Zugparameter
        // -------------------------------
        MapZugparameter(bm, dto);

        // Fahrplanjahr ermitteln
        var fahrplanJahr = ResolveFahrplanjahr(dto);

        if (fahrplanJahr == null) {
            throw new InvalidOperationException(
                $"Fahrplanjahr konnte nicht bestimmt werden (Datei {sourceFile})");
        }

        dto.Vorgang.FahrplanJahr = fahrplanJahr.Value;

        DebugTraceHelper.TraceDocumentRegions(
            Logger,
            "Mapper.End",
            dto);

        return dto;
    }

    private void MapHaltausfall(ZvFExportBaumassnahmenBaumassnahme bm, FploXmlDocumentDto dto) {
        var has = bm.Zuege?.Haltausfall;
        if (has == null) return;

        foreach (var ha in has) {
            var basisVerkehrstag =
                DateOnly.FromDateTime(ha.Verkehrstag);

            var haDto = new FploHaltausfallDto {
                ZugNr                 = ha.Zugnr,
                Verkehrstag           = basisVerkehrstag,
                AusfallenderHaltDs100 = ha.AusfallenderHalt?.Ds100 ?? string.Empty,
                ErsatzHaltDs100       = ha.Ersatzhalt?.Ds100       ?? string.Empty,
            };

            dto.Document.Haltausfall.Add(haDto);
        }
    }

    private void MapZurueckgehalten(
        ZvFExportBaumassnahmenBaumassnahme bm,
        FploXmlDocumentDto                 dto) {
        var zgs = bm.Zuege?.Zurueckgehalten;
        if (zgs == null)
            return;

        foreach (var zg in zgs) {
            var verkehrstag = DateOnly.FromDateTime(zg.Verkehrstag);

            var zurueckhaltenBis = BuildZurueckhaltenBis(verkehrstag, zg.ZurueckhaltenBis);

            var zgDto = new FploZurueckgehaltenDto {
                ZugNr            = zg.Zugnr,
                Verkehrstag      = verkehrstag,
                AbBstDs100       = zg.AbBst.Ds100 ?? string.Empty,
                ZurueckhaltenBis = zurueckhaltenBis
            };

            dto.Document.Zurueckgehalten.Add(zgDto);
        }
    }

    private void MapZugparameter(ZvFExportBaumassnahmenBaumassnahme bm, FploXmlDocumentDto dto) {
        var zps = bm.Zuege?.Zugparameter;
        if (zps == null) return;

        foreach (var zp in zps) {
            var basisVerkehrstag =
                DateOnly.FromDateTime(zp.Verkehrstag);

            var zpDto = new FploZugparameterDto {
                ZugNr            = zp.Zugnr,
                Verkehrstag      = basisVerkehrstag,
                WirktAbBstDs100  = zp.WirktAbBst?.Ds100  ?? string.Empty,
                WirktBisBstDs100 = zp.WirktBisBst?.Ds100 ?? string.Empty,
                Wert             = zp.Wert               ?? string.Empty,
                Art              = zp.Art                ?? string.Empty
            };

            dto.Document.Zugparameter.Add(zpDto);
        }
    }

    private void MapSev(
        ZvFExportBaumassnahmenBaumassnahme bm,
        FploXmlDocumentDto                 dto) {
        var sevs = bm.Zuege?.Sev;
        if (sevs == null)
            return;

        foreach (var sev in sevs) {
            var basisVerkehrstag =
                DateOnly.FromDateTime(sev.Verkehrstag);

            var sevDto = new FploSevDto {
                ZugNr       = sev.Zugnr,
                Verkehrstag = basisVerkehrstag,

                StartDs100 = sev.Startbf ?? string.Empty,
                EndDs100   = sev.Zielbf  ?? string.Empty,

                NeuerFahrplan = sev.NeuerFahrplan == 1,

                AusfallVonDs100 = sev.AusfallVon?.Ds100,
                AusfallVonName  = sev.AusfallVon?.Value,

                AusfallBisDs100 = sev.AusfallBis?.Ds100,
                AusfallBisName  = sev.AusfallBis?.Value
            };

            // -------------------------------
            // Ersatzzug (optional)
            // -------------------------------
            if (sev.Ersatzzug != null) {
                var ez = sev.Ersatzzug;

                sevDto.Ersatzzug = new FploErsatzZugDto {
                    Zugnummer   = ez.Zugnr,
                    Verkehrstag = DateOnly.FromDateTime(ez.Verkehrstag),

                    AbgangDs100 = ez.Startbf,
                    ZielDs100   = ez.Zielbf,

                    NeuerFahrplan = ez.NeuerFahrplan == 1
                };
            }

            dto.Document.Sev.Add(sevDto);
        }
    }

    private void MapZuege(
        ZvFExportBaumassnahmenBaumassnahme bm,
        FploXmlDocumentDto                 dto) {
        var zuege = bm.Zuege?.Zug;
        if (zuege == null) return;

        foreach (var zug in zuege) {
            var basisVerkehrstag =
                DateOnly.FromDateTime(zug.Verkehrstag);

            var tagwechsel =
                int.TryParse(zug.Tageswechsel, out var tw) ? tw : 0;

            var effektiverVerkehrstag =
                VerkehrstagHelper.ApplyTagwechsel(
                    basisVerkehrstag,
                    tagwechsel);

            var zugDto = new FploZugDto {
                Zugnummer   = zug.Zugnr,
                Verkehrstag = effektiverVerkehrstag,

                Zugbez     = zug.Zugbez ?? string.Empty,
                ZugGattung = zug.Zuggat,
                Betreiber  = zug.Betreiber ?? string.Empty,

                Bedarf                 = zug.Bedarf == 1,
                IstSicherheitsrelevant = zug.SicherheitsrelevanterzugSpecified,
                LauterZug              = zug.LauterzugSpecified,
                Vmax                   = ParseLong(zug.Vmax),
                Tfzf                   = zug.Tfz,
                Last                   = ParseLong(zug.Last),
                Laenge                 = ParseLong(zug.Laenge),
                Bremssystem            = zug.Brems,
                Ebula                  = ParseBool(zug.Ebula),
                Skl                    = zug.Skl ?? string.Empty,
                Klv                    = zug.Klv ?? string.Empty,

                FploAbschnitt = zug.FploAbschnitt,

                AbgangDs100 = zug.Regelweg?.Abgangsbahnhof?.Ds100 ?? "",
                ZielDs100   = zug.Regelweg?.Zielbahnhof?.Ds100    ?? "",

                Regelweg = zug.Regelweg,
                Fahrplan = MapFahrplan(zug)
            };

            dto.Document.ZuegeRaw.Add(zugDto);
        }
    }

    private static List<FploZugFahrplanDto> MapFahrplan(
        ZvFExportBaumassnahmenBaumassnahmeZuegeZug zug) {
        var result = new List<FploZugFahrplanDto>();

        if (zug.Fahrplan?.Fahrplanzeit == null)
            return result;

        var verkehrstag = DateOnly.FromDateTime(zug.Verkehrstag);

        foreach (var fp in zug.Fahrplan.Fahrplanzeit) {
            var ankunft = BuildFahrplanDateTime(
                verkehrstag,
                fp.Ankunft,
                fp.Tagwechsel);

            var abfahrt = BuildFahrplanDateTime(
                verkehrstag,
                fp.Abfahrt,
                fp.Tagwechsel);

            result.Add(new FploZugFahrplanDto {
                LfdNr              = fp.LfdNr,
                BstDs100           = fp.Bahnhof ?? string.Empty,
                HalteArt           = fp.Haltart ?? string.Empty,
                AnkunftsZeit       = ankunft,
                AbfahrtsZeit       = abfahrt,
                Bemerkung          = fp.Bemerkung         ?? string.Empty,
                EbulaVglZug        = fp.Bfpl?.Ebulavglzug ?? string.Empty,
                EbulaVglMbr        = ParseInt(fp.Bfpl?.Ebulavglmbr),
                EbulaVglBrs        = fp.Bfpl?.Ebulavglbrs,
                EbulaFahrplanHeft  = ParseInt(fp.Bfpl?.Efplh),
                EbulaFahrplanSeite = ParseInt(fp.Bfpl?.Efpls)
            });
        }

        return result;
    }


    private static int? ResolveFahrplanjahr(FploXmlDocumentDto dto) {
        // 1️⃣ Baudatum
        var fy = FahrplanjahrHelper.FromDateRange(
            dto.Document.GueltigAb,
            dto.Document.GueltigBis);

        if (fy != null)
            return fy;

        // 2️⃣ kleinster Verkehrstag aus Zügen
        var minZugTag = dto.Document.ZuegeRaw
            .Select(z => z.Verkehrstag)
            .Where(d => d != default)
            .DefaultIfEmpty()
            .Min();

        if (minZugTag != default)
            return FahrplanjahrHelper.FromDate(minZugTag);

        return null;
    }

    private static void MapStreckenabschnitte(
        ZvFExportBaumassnahmenBaumassnahme bm,
        FploXmlDocumentDto                 dto) {
        var src = bm.Streckenabschnitte;
        if (src == null)
            return;

        // Wenn du SharedStreckeDto nutzt (aus Teil 2), füllen wir dto.Document.Strecken.
        // Falls deine DocumentDTO anders heißt: entsprechend anpassen.
        foreach (var s in src) {
            dto.Document.Strecken.Add(new SharedStreckeDto {
                Grund = s.Grund,
                Vzg = s.VzGListe != null && s.VzGListe.Length > 0
                    ? string.Join(",", s.VzGListe)
                    : null,
                Export               = s.Export,
                Massnahme            = s.Massnahme,
                StartBst             = s.Startbst,
                EndBst               = s.Endbst,
                Betriebsweise        = s.Betriebsweise,
                Baubeginn            = s.Baubeginn,
                Bauende              = s.Bauende,
                ZeitraumUnterbrochen = s.ZeitraumUnterbrochen == "Ja"
            });
        }
    }

    private static int? ParseInt(string? value)
        => int.TryParse(value, out var i) ? i : null;

    private static long? ParseLong(string? value)
        => long.TryParse(value, out var l) ? l : null;

    private static bool ParseBool(string? value)
        => value == "1";

    private static TimeOnly? ParseTimeOnly(string? value) {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (value.StartsWith("T"))
            value = value[1..];

        return TimeOnly.TryParse(value, out var t)
            ? t
            : null;
    }

    private static DateTime? ToDateTime(
        DateOnly  verkehrstag,
        TimeOnly? time,
        int       dayOffset) {
        if (!time.HasValue)
            return null;

        return verkehrstag
            .AddDays(dayOffset)
            .ToDateTime(time.Value);
    }

    private static DateTime BuildZurueckhaltenBis(
        DateOnly verkehrstag,
        string   timeRaw) {
        if (string.IsNullOrWhiteSpace(timeRaw))
            throw new ArgumentException("ZurueckhaltenBis fehlt");

        return !TimeOnly.TryParse(timeRaw, out var bis)
            ? throw new FormatException($"Ungültige Zeitangabe: '{timeRaw}'")
            : verkehrstag.ToDateTime(bis);
    }

    private static DateTime? BuildFahrplanDateTime(
        DateOnly verkehrstag,
        string?  timeRaw,
        int      tagwechsel) {
        if (string.IsNullOrWhiteSpace(timeRaw))
            return null;

        // XML kommt als "T04:18:00"
        if (!TimeOnly.TryParse(timeRaw.TrimStart('T'), out var time))
            throw new FormatException($"Ungültige Zeitangabe: '{timeRaw}'");

        var date = verkehrstag.AddDays(tagwechsel);

        return date.ToDateTime(time);
    }

    private static readonly Regex TeillieferungRegex = new(@"_T\d+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex NachtragRegex = new(@"_N\d+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
       
    private static bool IsEntwurfFile(string fileName) {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        // akzeptiert: "Fplo-E 123...", "FPLO-E_123...", "fplo-e-123..." etc.
        return Regex.IsMatch(
            fileName.TrimStart(),
            @"^Fplo\s*-\s*E\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static void MapDateiFlags(string dateiname, FploDocumentDto dto) {
        if (string.IsNullOrWhiteSpace(dateiname))
            return;

        dto.IstEntwurf = IsEntwurfFile(dateiname);

        // Teillieferung: _T1, _T12, ...
        dto.IstTeillieferung = TeillieferungRegex.IsMatch(dateiname);

        // Nachtrag: _N1, _N12, ...
        dto.IstNachtrag = NachtragRegex.IsMatch(dateiname);
    }
}