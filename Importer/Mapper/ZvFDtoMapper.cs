using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using BauFahrplanMonitor.Importer.Dto.Shared;
using BauFahrplanMonitor.Importer.Dto.ZvF;
using BauFahrplanMonitor.Importer.Helper;
using BauFahrplanMonitor.Importer.Interface;
using BauFahrplanMonitor.Importer.Xml;
using NLog;

namespace BauFahrplanMonitor.Importer.Mapper;

public sealed class ZvFDtoMapper : IZvFDtoMapper {
    private static readonly Logger Logger =
        LogManager.GetCurrentClassLogger();

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
        dto.Header.FileName        = export.Header.Filename ?? sourceFile;
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
        dto.Vorgang.FahrplanJahr =
            FahrplanjahrHelper.FromDateRange(
                dto.Document.BauDatumVon,
                dto.Document.BauDatumBis);

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

            if (Logger.IsDebugEnabled) {
                var json = JsonSerializer.Serialize(
                    abw,
                    new JsonSerializerOptions {
                        WriteIndented          = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        Encoder                = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });

                //Logger.Debug($"ZvF (Abw) nach Mapping ({zug.Zugnr}/{zug.Verkehrstag}):\n{json}");
            }

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
            var (verkehrstag, tagwechsel) =
                ParseVerkehrstag(e.Verkehrstag);

            dto.Document.Entfallen.Add(new ZvFZugEntfallenDto {
                Zugnr           = e.Zugnr,
                Zugbez          = e.Zugbez ?? "",
                Verkehrstag     = verkehrstag,
                RegelungsartAlt = e.RegelungsArtalt ?? ""
            });
        }
    }

    private static (DateOnly Verkehrstag, int Tagwechsel)
        ParseVerkehrstag(string raw) {
        if (string.IsNullOrWhiteSpace(raw))
            throw new ArgumentException("Verkehrstag leer");

        // Normalfall: einzelnes Datum
        if (DateOnly.TryParse(raw, out var single))
            return (single, 0);

        // Sonderfall: dd./dd.MM.yy  -> Verkehrstag = erster Tag, Tagwechsel = +1
        var m = Regex.Match(
            raw,
            @"^(?<d1>\d{2})\./(?<d2>\d{2})\.(?<m>\d{2})\.(?<y>\d{2})$");

        if (!m.Success)
            throw new FormatException($"Unbekanntes Verkehrstag-Format: '{raw}'");

        var day1  = int.Parse(m.Groups["d1"].Value);
        var month = int.Parse(m.Groups["m"].Value);
        var year  = 2000 + int.Parse(m.Groups["y"].Value);

        return (new DateOnly(year, month, day1), +1);
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
                ZeitraumUnterbrochen = s.ZeitraumUnterbrochen
            });
        }

        // JSON erzeugen (für jsonb in zvf_dokument_streckenabschnitte)
        var jsonOptions = new JsonSerializerOptions {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy   = JsonNamingPolicy.CamelCase,
            WriteIndented          = false
        };

        dto.Document.StreckenJson = JsonSerializer.Serialize(dto.Document.Strecken, jsonOptions);
    }
}