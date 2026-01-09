using System.Text.Encodings.Web;
using System.Text.Json;
using BauFahrplanMonitor.Core.Importer.Dto.ZvF;
using BauFahrplanMonitor.Core.Importer.Dto.ZvF.Abweichungen;
using BauFahrplanMonitor.Core.Importer.Xml;
using NLog;

namespace BauFahrplanMonitor.Core.Importer.Mapper;

/// <summary>
/// Factory zur Erzeugung strukturierter ZvF-Abweichungen
/// aus der XML-Rohdarstellung.
///
/// Verantwortlich für:
///  - Interpretation der XML-Abweichungsstruktur
///  - Auswahl des passenden Abweichungs-Typs anhand der Regelungsart
///  - Konvertierung in typisierte Abweichungs-DTOs
///  - Serialisierung der Abweichung als JSON
///
/// NICHT verantwortlich für:
///  - Auflösung von Betriebsstellen-IDs
///  - Persistenz oder Datenbankzugriffe
///  - Fachliche Bewertung der Regelung
/// </summary>
/// <remarks>
/// Diese Factory wird von der <see cref="ZvFBusinessLogic"/> verwendet.
///
/// Die erzeugten Abweichungen enthalten:
///  - eine einheitliche Regelungsart
///  - eine JSON-Repräsentation der Details
///  - eine Anchor-RL100 (DS100), die später im Upsert
///    in eine DB-Referenz aufgelöst wird.
///
/// Die eigentliche Typisierung der Abweichung
/// erfolgt ausschließlich hier.
/// </remarks>
public static class ZvFAbweichungFactory {
    private static readonly Logger Logger =
        LogManager.GetCurrentClassLogger();
    /// <summary>
    /// Serializer-Optionen für kompakte JSON-Abweichungen.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOpts = new() {
        WriteIndented = false,
        Encoder       = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    // =====================================================================
    // ENTRYPOINT
    // =====================================================================

    /// <summary>
    /// Erzeugt ein <see cref="ZvFAbweichungDto"/> aus einer XML-Abweichung.
    /// </summary>
    /// <param name="src">XML-Abweichungsknoten</param>
    /// <param name="zugnummer">Zugnummer</param>
    /// <param name="verkehrstag">Verkehrstag</param>
    /// <param name="ankerBstRef">
    /// Platzhalter für die Betriebsstellen-Referenz
    /// (wird später im Upsert aufgelöst)
    /// </param>
    /// <returns>
    /// Strukturierte Abweichung oder <c>null</c>, wenn keine Abweichung vorliegt.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Wird geworfen, wenn die Regelungsart unbekannt ist.
    /// </exception>
    public static ZvFZugAbweichung? Create(
        ZvFExportBaumassnahmenBaumassnahmeZuegeZugAbweichung? src,
        long zugnummer,
        DateOnly verkehrstag
    ) {
        if (src == null)
            return null;

        var art = src.Art?.Trim().ToLowerInvariant() ?? "";

        // --------------------------------------
        // Regelungsart auswählen
        // --------------------------------------
        object dto = art switch {
            "umleitung"                   => ConvertUmleitung(src),
            "ersatzhalte"                 => ConvertErsatzhalte(src),
            "verspaetung" or "verspätung" => ConvertVerspaetung(src),
            "vorplan"                     => ConvertVorplan(src),
            "ausfall"                     => ConvertAusfall(src),
            "regelung"                    => ConvertRegelung(src),
            _                             => throw new NotSupportedException($"Unbekannte Regelungsart: {art}")
        };

        var json = JsonSerializer.Serialize(dto, JsonOpts);

        // --------------------------------------
        // Anker RL100 bestimmen (wird später in DB-ID umgewandelt)
        // --------------------------------------
        var anchorRl100 = art switch {
            "umleitung" => src.Umleitweg is { Length: > 0 } ? src.Umleitweg[0] : null,
            "ersatzhalte" => src.Haltliste is { Length: > 0 } ? src.Haltliste[0].Ausfall?.Ds100 : null,
            "verspaetung" or "verspätung" => src.Verspaetungab?.Ds100,
            "vorplan" => src.Vorplanab?.Ds100,
            "ausfall" => src.Ausfallvon?.Ds100,
            "regelung" => src.Regelungsliste is { Length: > 0 } ? src.Regelungsliste[0].GiltIn?.Ds100 : null,
            _ => throw new Exception($"Keinen passenden Anker gefunden für {art}")
        };

        if (string.IsNullOrWhiteSpace(anchorRl100)) {
            Logger.Warn(
                "ZvFAbweichung ohne Anker: Art={0}, Zugnr={1}, Verkehrstag={2}",
                art,
                zugnummer,
                verkehrstag);
        }
        
        // ------------------------------------------------------------
        // 4) Ergebnis-DTO
        // ------------------------------------------------------------
        return new ZvFZugAbweichung {
            Zugnummer    = zugnummer,
            Verkehrstag  = verkehrstag,
            Regelungsart = art,
            JsonRaw      = json,
            AnchorRl100  = anchorRl100,
        };
    }

    // =====================================================================
    // KONVERTER
    // =====================================================================

    /// <summary>
    /// Konvertiert eine Umleitungs-Abweichung.
    /// </summary>
    private static ZvFAbweichungUmleitungDto ConvertUmleitung(
        ZvFExportBaumassnahmenBaumassnahmeZuegeZugAbweichung s) {
        var dto = new ZvFAbweichungUmleitungDto {
            Umleitung = s.Umleitung ?? "",
            PrognostizierteVerspaetung =
                int.TryParse(s.Verspaetung, out var v) ? v : 0
        };

        if (s.Umleitweg != null)
            dto.UmleitwegRl100.AddRange(s.Umleitweg);

        return dto;
    }

    /// <summary>
    /// Konvertiert Ersatzhalte (Haltausfälle mit Ersatz).
    /// </summary>
    private static ZvFAbweichungErsatzhalteDto ConvertErsatzhalte(
        ZvFExportBaumassnahmenBaumassnahmeZuegeZugAbweichung s) {
        var dto = new ZvFAbweichungErsatzhalteDto();

        if (s.Haltliste == null)
            return dto;
        foreach (var h in s.Haltliste) {
            dto.Halteliste.Add(new ZvFAbweichungHaltausfallDto {
                Folge        = h.Folge,
                AusfallRl100 = h.Ausfall?.Ds100,
                ErsatzRl100  = h.Ersatz?.Ds100
            });
        }

        return dto;
    }

    /// <summary>
    /// Konvertiert eine Verspätungs-Abweichung.
    /// </summary>
    private static ZvFAbweichungVerspaetungDto ConvertVerspaetung(
        ZvFExportBaumassnahmenBaumassnahmeZuegeZugAbweichung s) {
        return new ZvFAbweichungVerspaetungDto {
            Verspaetung        = int.TryParse(s.Verspaetung, out var v) ? v : 0,
            VerspaetungAbRl100 = s.Verspaetungab?.Ds100
        };
    }

    /// <summary>
    /// Konvertiert eine Vorplan-Abweichung.
    /// </summary>
    /// <remarks>
    /// Vorplan wird als negative Verspätung interpretiert.
    /// </remarks>
    private static ZvFAbweichungVorplanDto ConvertVorplan(
        ZvFExportBaumassnahmenBaumassnahmeZuegeZugAbweichung s) {
        return new ZvFAbweichungVorplanDto {
            Vorplan        = int.TryParse(s.Verspaetung, out var v) ? -v : 0,
            VorplanAbRl100 = s.Vorplanab?.Ds100
        };
    }

    /// <summary>
    /// Konvertiert einen Zugausfall.
    /// </summary>
    private static ZvFAbweichungAusfallDto ConvertAusfall(
        ZvFExportBaumassnahmenBaumassnahmeZuegeZugAbweichung s) {
        return new ZvFAbweichungAusfallDto {
            AusfallAbRl100  = s.Ausfallvon?.Ds100,
            AusfallBisRl100 = s.Ausfallbis?.Ds100
        };
    }

    /// <summary>
    /// Konvertiert eine allgemeine Regelungsliste (WZR).
    /// </summary>
    private static ZvFAbweichungWzrListeDto ConvertRegelung(
        ZvFExportBaumassnahmenBaumassnahmeZuegeZugAbweichung s) {
        var dto = new ZvFAbweichungWzrListeDto();

        if (s.Regelungsliste == null)
            return dto;
        foreach (var r in s.Regelungsliste) {
            dto.RegelungListe.Add(new ZvFAbweichungWzrDto {
                Art         = r.Art,
                GiltInRl100 = r.GiltIn?.Ds100,
                Text        = r.Text
            });
        }

        return dto;
    }
}