using BauFahrplanMonitor.Core.Domain.Infrastruktur;
using BauFahrplanMonitor.Trassenfinder.Generated;
using Domain = BauFahrplanMonitor.Core.Domain.Infrastruktur;
using Dto = BauFahrplanMonitor.Trassenfinder.Generated;

namespace BauFahrplanMonitor.Trassenfinder.Mapping;

internal static class TrassenfinderInfrastrukturMapper {
    public static InfrastrukturElement Map(Infrastruktur_summary dto) {
        return new InfrastrukturElement {
            Id           = dto.Id.ToString(),
            Bezeichnung  = dto.Anzeigename,
            FahrplanJahr = dto.Fahrplanjahr,
            GueltigVon   = dto.Gueltig_von,
            GueltigBis   = dto.Gueltig_bis
        };
    }

    public static Domain.Infrastruktur Map(Dto.Infrastruktur dto) {
        return new Domain.Infrastruktur {
            Id           = dto.Id.ToString(),
            Anzeigename  = dto.Anzeigename,
            Fahrplanjahr = dto.Fahrplanjahr,
            GueltigVon   = DateOnly.FromDateTime(dto.Gueltig_von.DateTime),
            GueltigBis   = DateOnly.FromDateTime(dto.Gueltig_bis.DateTime),
            Betriebsstellen = dto.Ordnungsrahmen.Betriebsstellen
                .Select(Map)
                .ToList(),
            Triebfahrzeuge = dto.Stammdaten.Triebfahrzeuge
                .Select(Map)
                .ToList()
        };
    }

    private static Domain.Betriebsstelle Map(Dto.Betriebsstelle dto) {
        return new Domain.Betriebsstelle {
            Ds100          = dto.Ds100,
            Name           = dto.Langname,
            Plc            = dto.Primary_location_code,
            Breite         = dto.Geo_koordinaten?.Breite,
            Laenge         = dto.Geo_koordinaten?.Laenge,
            Elektrifiziert = dto.Elektrifiziert,
            IstBahnhof     = dto.Bahnhof
        };
    }

    private static Domain.Triebfahrzeug Map(Dto.Triebfahrzeug dto) {
        return new Domain.Triebfahrzeug {
            Baureihe           = $"{dto.Hauptnummer}.{dto.Unternummer}",
            Bezeichnung        = dto.Bezeichnung,
            Elektrifiziert     = dto.Elektrifiziert,
            Triebwagen         = dto.Triebwagen,
            AktiveNeigetechnik = dto.Aktive_neigetechnik,
            KennungWert        = dto.Kennung_wert
        };
    }

}