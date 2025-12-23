using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using BauFahrplanMonitor.Importer.Dto.Shared;
using BauFahrplanMonitor.Importer.Dto.ZvF;
using BauFahrplanMonitor.Importer.Helper;
using BauFahrplanMonitor.Importer.Interface;
using BauFahrplanMonitor.Importer.Xml;

namespace BauFahrplanMonitor.Importer.Mapper;

public sealed class ZvFDtoMapper : IZvFDtoMapper {
    public ZvFXmlDocumentDto Map(ZvFExport export, string sourceFile) {
        if (export.Header == null)
            throw new InvalidOperationException("ZvFExport.Header fehlt");

        if (export.Baumassnahmen?.Baumassnahme == null)
            throw new InvalidOperationException("Keine Baumassnahme vorhanden");

        var bm = export.Baumassnahmen.Baumassnahme;

        var dto = new ZvFXmlDocumentDto();

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
        dto.Document.BauDatumVon = bm.BauDatumVon != null
            ? DateOnly.FromDateTime(bm.BauDatumVon.Value)
            : null;
        dto.Document.BauDatumBis = bm.BauDatumBis != null
            ? DateOnly.FromDateTime(bm.BauDatumBis.Value)
            : null;
        dto.Document.AntwortBis = bm.Antwort != null
            ? DateOnly.FromDateTime(bm.Antwort.Value)
            : null;
        dto.Document.Endstueck     = bm.EndStueckZvf == 1;
        dto.Document.AllgemeinText = string.Join("\n", bm.Allgregelungen ?? []);

        // -------------------------------
        // Vorgang
        // -------------------------------
        dto.Vorgang.Extension  = bm.Extension                        ?? "";
        dto.Vorgang.Korridor   = bm.Korridor                         ?? "";
        dto.Vorgang.KigBau     = bm.Kigbau                           ?? "";
        dto.Vorgang.IstQs      = bm.Qsbaumassnahme?.StartsWith($"Q") ?? false;
        dto.Vorgang.IstKs      = bm.Qsbaumassnahme?.StartsWith($"K") ?? false;
        dto.Vorgang.MasterFplo = bm.MasterFplo;


        // -------------------------------
        // BBMN
        // -------------------------------
        var bbmns = new List<string>();
        if (!string.IsNullOrEmpty(bm.Kennung))
            foreach (var bbmn in bm.Kennung.Split(',')) {
                bbmns.Add(bbmn);
            }

        dto.Vorgang.Bbmn = bbmns
            .Where(b => !string.IsNullOrWhiteSpace(b))
            .Select(b => b.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // -------------------------------
        // Streckenabschnitte → SharedStreckeDto + JSON
        // -------------------------------
        MapStreckenabschnitte(bm, dto);

        // -------------------------------
        // Züge / Entfallen
        // -------------------------------
        MapZuege(bm, dto);
        MapEntfalleneZuege(bm, dto);


        // Fahrplanjahr ermitteln
        var fahrplanJahr = ResolveFahrplanjahr(dto);

        if (fahrplanJahr == null) {
            throw new InvalidOperationException(
                $"Fahrplanjahr konnte nicht bestimmt werden (Datei {sourceFile})");
        }

        dto.Vorgang.FahrplanJahr = fahrplanJahr.Value;

        return dto;
    }

    private static void MapZuege(
        ZvFExportBaumassnahmenBaumassnahme bm,
        ZvFXmlDocumentDto                  dto) {
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

            var zugDto = new ZvFZugDto {
                Zugnummer    = zug.Zugnr,
                Verkehrstag  = effektiverVerkehrstag,
                Zugbez       = zug.Zugbez                          ?? "",
                Betreiber    = zug.Betreiber                       ?? "",
                AbgangDs100  = zug.Regelweg?.Abgangsbahnhof?.Ds100 ?? "",
                ZielDs100    = zug.Regelweg?.Zielbahnhof?.Ds100    ?? "",
                LinienNr     = zug.Regelweg?.LinienNr              ?? "",
                Tageswechsel = tagwechsel,
                Regelweg     = zug.Regelweg,
                Bemerkung    = zug.Bemerkung,
                Bemerkungen  = string.Join("\n", zug.Bemerkung),
                Bedarf       = zug.Bedarf == 1,
                Sonder       = zug is { SonderSpecified: true, Sonder: 1 },
                Aenderung    = zug.Aenderung
            };

            var abw = ZvFAbweichungFactory.Create(
                zug.Abweichung,
                zug.Zugnr,
                effektiverVerkehrstag
            );

            if (abw != null)
                zugDto.Abweichungen.Add(abw);


            dto.Document.ZuegeRaw.Add(zugDto);
        }
    }

    private static void MapEntfalleneZuege(
        ZvFExportBaumassnahmenBaumassnahme bm,
        ZvFXmlDocumentDto                  dto) {
        var entfallen = bm.Zuege?.Entfallen;
        if (entfallen == null) return;

        foreach (var e in entfallen) {
            var verkehrstag = ParseVerkehrstag(e.Verkehrstag);

            dto.Document.Entfallen.Add(new ZvFZugEntfallenDto {
                Zugnr           = e.Zugnr,
                Zugbez          = e.Zugbez ?? "",
                Verkehrstag     = verkehrstag,
                RegelungsartAlt = e.RegelungsArtalt ?? ""
            });
        }
    }

    private static DateOnly ParseVerkehrstag(string raw) {
        if (string.IsNullOrWhiteSpace(raw))
            throw new FormatException("Verkehrstag leer");

        var input = raw.Trim();

        string day;
        string month;
        string year;

        if (input.Contains('/')) {
            // z.B.:
            // 31.05./01.06.25
            // 28.02./01.03.25
            // 24./25.01.25

            var parts = input.Split('/', StringSplitOptions.RemoveEmptyEntries);

            var left  = parts[0].Trim();  // "31.05." oder "24."
            var right = parts[^1].Trim(); // "01.06.25" oder "25.01.25"

            var rightParts = right.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (rightParts.Length < 3)
                throw new FormatException($"Unbekanntes Verkehrstag-Format: '{raw}'");

            year = rightParts[2];

            var leftParts = left.Split('.', StringSplitOptions.RemoveEmptyEntries);

            day = leftParts[0];

            // 🔑 linker Monat vorhanden?
            if (leftParts.Length >= 2 && !string.IsNullOrWhiteSpace(leftParts[1])) {
                month = leftParts[1];
            }
            else {
                // Monat aus rechtem Teil übernehmen
                month = rightParts[1];
            }
        }
        else {
            // Einfaches Datum: dd.MM.yy
            var parts = input.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
                throw new FormatException($"Unbekanntes Verkehrstag-Format: '{raw}'");

            day   = parts[0];
            month = parts[1];
            year  = parts[2];
        }

        var normalized = $"{day}.{month}.{year}";

        return DateOnly.TryParseExact(
            normalized,
            "dd.MM.yy",
            CultureInfo.GetCultureInfo("de-DE"),
            DateTimeStyles.None,
            out var result)
            ? result
            : throw new FormatException($"Unbekanntes Verkehrstag-Format: '{raw}'");
    }

    private static int? ResolveFahrplanjahr(ZvFXmlDocumentDto dto) {
        // 1️⃣ Baudatum
        var fy = FahrplanjahrHelper.FromDateRange(
            dto.Document.BauDatumVon,
            dto.Document.BauDatumBis);

        if (fy != null)
            return fy;

        // 2️⃣ kleinster Verkehrstag aus Zügen
        var minZugTag = dto.Document.ZuegeRaw?
            .Select(z => z.Verkehrstag)
            .Where(d => d != default)
            .DefaultIfEmpty()
            .Min();

        if (minZugTag != default)
            return FahrplanjahrHelper.FromDate(minZugTag.Value);

        // 3️⃣ kleinster Verkehrstag aus Entfallen
        var minEntfallTag = dto.Document.Entfallen?
            .Select(e => e.Verkehrstag)
            .Where(d => d != default)
            .DefaultIfEmpty()
            .Min();

        if (minEntfallTag != default)
            return FahrplanjahrHelper.FromDate(minEntfallTag.Value);

        // ❌ nichts verwertbares vorhanden
        return null;
    }
    
    private static void MapStreckenabschnitte(
        ZvFExportBaumassnahmenBaumassnahme bm,
        ZvFXmlDocumentDto                  dto) {
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
                Vzg                  = string.Join(",", s.VzGListe),
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
}