using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BauFahrplanMonitor.Importer.Dto.Shared;
using BauFahrplanMonitor.Importer.Dto.Ueb;
using BauFahrplanMonitor.Importer.Helper;
using BauFahrplanMonitor.Importer.Interface;
using BauFahrplanMonitor.Importer.Xml;

namespace BauFahrplanMonitor.Importer.Mapper;

public sealed class UeBDtoMapper : IUeBDtoMapper {
    public UebXmlDocumentDto Map(ZvFExport export, string sourceFile) {
        if (export.Header == null)
            throw new InvalidOperationException("ZvFExport.Header fehlt");

        if (export.Baumassnahmen?.Baumassnahme == null)
            throw new InvalidOperationException("Keine Baumassnahme vorhanden");

        var bm = export.Baumassnahmen.Baumassnahme;

        var dto = new UebXmlDocumentDto();

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
        
        if(export.Header.Empfaengerliste is { Length: > 0 })
            foreach(var e in export.Header.Empfaengerliste) 
                dto.Header.Empfaenger.Add(e);

        // -------------------------------
        // Dokument (Meta)
        // -------------------------------
        var fileName = Path.GetFileName(sourceFile);
        dto.Document.Dateiname           = fileName;
        dto.Document.Masterniederlassung = bm.Masterniederlassung ?? "";
        dto.Document.ExportTimestamp     = export.Header.Timestamp;
        dto.Document.Version.Major       = bm.Version?.Major ?? 0;
        dto.Document.Version.Minor       = bm.Version?.Minor ?? 0;
        dto.Document.Version.Sub         = bm.Version?.Sub   ?? 0;
        dto.Document.Version.VersionNumeric = dto.Document.Version.Major * 1000 + 
                                              dto.Document.Version.Minor * 100 +
                                              dto.Document.Version.Sub;
        
        dto.Document.GueltigAb = bm.GueltigkeitFplo?.Beginn != null
            ? DateOnly.FromDateTime(bm.GueltigkeitFplo.Beginn)
            : null;
        dto.Document.GueltigBis = bm.GueltigkeitFplo?.Ende != null
            ? DateOnly.FromDateTime(bm.GueltigkeitFplo.Ende)
            : null;
        dto.Document.AllgemeinText = string.Join("\n", bm.Allgregelungen ?? []);

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

        // Fahrplanjahr ermitteln
        var fahrplanJahr = ResolveFahrplanjahr(dto);

        if (fahrplanJahr == null) {
            throw new InvalidOperationException(
                $"Fahrplanjahr konnte nicht bestimmt werden (Datei {sourceFile})");
        }

        dto.Vorgang.FahrplanJahr = fahrplanJahr.Value;

        return dto;
    }

    private static List<UebKnotenDto> MapKnotenzeiten(
        ZvFExportBaumassnahmenBaumassnahmeZuegeZug zug) {
        var result = new List<UebKnotenDto>();

        var knoten = zug.Knotenzeiten?.Knotenzeit;
        if (knoten == null || knoten.Length == 0)
            return result;

        var verkehrstag = DateOnly.FromDateTime(zug.Verkehrstag);

        TimeOnly? lastSeenTime = null;
        var       dayOffset    = 0;

        foreach (var k in knoten) {
            // -------------------------
            // Ankunft
            // -------------------------
            var ankunftTime = ParseTimeOnly(k.Ankunft);

            if (lastSeenTime.HasValue && ankunftTime.HasValue &&
                ankunftTime.Value < lastSeenTime.Value) {
                dayOffset++;
            }

            var ankunft = ToDateTime(
                verkehrstag,
                ankunftTime,
                dayOffset);

            if (ankunftTime.HasValue)
                lastSeenTime = ankunftTime;

            // -------------------------
            // Abfahrt
            // -------------------------
            var abfahrtTime = ParseTimeOnly(k.Abfahrt);

            if (lastSeenTime.HasValue && abfahrtTime.HasValue &&
                abfahrtTime.Value < lastSeenTime.Value) {
                dayOffset++;
            }

            var abfahrt = ToDateTime(
                verkehrstag,
                abfahrtTime,
                dayOffset);

            if (abfahrtTime.HasValue)
                lastSeenTime = abfahrtTime;

            // -------------------------
            // DTO
            // -------------------------
            result.Add(new UebKnotenDto {
                BahnhofDs100 = k.Bahnhof,
                Haltart      = k.Haltart,
                AnkunftsZeit = ankunft,
                Abfahrtszeit = abfahrt,
                RelativLage  = ParseInt(k.Relativlage) ?? 0
            });
        }

        return result;
    }

    private void MapSev(
        ZvFExportBaumassnahmenBaumassnahme bm,
        UebXmlDocumentDto                  dto) {
        var sevs = bm.Zuege?.Sev;
        if (sevs == null)
            return;

        foreach (var sev in sevs) {
            var basisVerkehrstag =
                DateOnly.FromDateTime(sev.Verkehrstag);

            var sevDto = new UebSevDto {
                ZugNr       = sev.Zugnr,
                Verkehrstag = basisVerkehrstag,

                StartDs100 = sev.Startbf ?? string.Empty,
                EndDs100   = sev.Zielbf ?? string.Empty,

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

                sevDto.Ersatzzug = new UebErsatzZugDto {
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


    private static void MapZuege(
        ZvFExportBaumassnahmenBaumassnahme bm,
        UebXmlDocumentDto                  dto) {
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

            var zugDto = new UebZugDto {
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

                Knotenzeiten = MapKnotenzeiten(zug) // ✅ HIER der Ersatz
            };


            dto.Document.ZuegeRaw.Add(zugDto);
        }
    }


    private static int? ResolveFahrplanjahr(UebXmlDocumentDto dto) {
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

        // ❌ nichts verwertbares vorhanden
        return null;
    }

    private static void MapStreckenabschnitte(
        ZvFExportBaumassnahmenBaumassnahme bm,
        UebXmlDocumentDto                  dto) {
        // ⚠️ Property-Namen ggf. an dein ZvFExport.cs anpassen:
        // Ich gehe davon aus, dass bm.Streckenabschnitte eine Liste/Collection ist.
        var src = bm.Streckenabschnitte;
        if (src == null)
            return;

        // Wenn du SharedStreckeDto nutzt (aus Teil 2), füllen wir dto.Document.Strecken.
        // Falls deine DocumentDTO anders heißt: entsprechend anpassen.
        foreach (var s in src) {
            dto.Document.Strecken.Add(new SharedStreckeDto {
                Grund                = s.Grund,
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
}