using BauFahrplanMonitor.Core.Data;
using BauFahrplanMonitor.Core.Dto;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BauFahrplanMonitor.Core.Services;

public sealed class ZugTimelineService {
    private readonly IDbContextFactory<UjBauDbContext> _dbFactory;

    public ZugTimelineService(
        IDbContextFactory<UjBauDbContext> dbFactory) {
        _dbFactory = dbFactory;
    }

    public ZugTimelineResult Build(
        int                              zugNr,
        DateOnly                         date,
        IReadOnlyList<RawZugTimelineRow> rows
    ) {
        if (rows.Count == 0)
            return new ZugTimelineResult(
                zugNr,
                date,
                Array.Empty<ZugTimelinePointDto>()
            );

        var timeline = new List<ZugTimelinePointDto>(rows.Count);

        foreach (var r in rows) {

            // --------------------------------------------------
            // 1️⃣ Zeiten normalisieren (NUR bei 00:00:00)
            // --------------------------------------------------
            TimeSpan arrivalTs   = r.PublishedArrival;
            TimeSpan departureTs = r.PublishedDeparture;

            // 00:00:00 gilt als "leer" (nur dann ersetzen)
            bool arrivalEmpty   = arrivalTs   == TimeSpan.Zero;
            bool departureEmpty = departureTs == TimeSpan.Zero;

            if (arrivalEmpty   && !departureEmpty) arrivalTs = departureTs;
            if (departureEmpty && !arrivalEmpty) departureTs = arrivalTs;

            // ---- entscheidend: Offset pro Zeit bestimmen ----
            // Default: beide im selben dayOffset (SQL bezieht sich auf base_time)
            int departureOffset = r.DayOffset;
            int arrivalOffset   = r.DayOffset;

            // Halt über Mitternacht: Arrival ist "größer" als Departure (23:59 > 00:01)
            // Dann gehört Arrival zum Vortag.
            if (r.DayOffset > 0 && arrivalTs > departureTs)
                arrivalOffset = r.DayOffset - 1;

            // Minuten berechnen
            int arrivalMinute =
                (int)arrivalTs.TotalMinutes + arrivalOffset * 1440;

            int departureMinute =
                (int)departureTs.TotalMinutes + departureOffset * 1440;

            // falls mal identisch/komisch: niemals negativ anzeigen
            if (departureMinute < arrivalMinute)
                departureMinute = arrivalMinute;

            // --------------------------------------------------
            // 3️⃣ DTO
            // --------------------------------------------------
            timeline.Add(new ZugTimelinePointDto(
                SeqNo: (int)r.SeqNo, // 👈 long → int
                DayOffset: r.DayOffset,
                ArrivalMinute: arrivalMinute,
                DepartureMinute: departureMinute,
                Arrival: TimeOnly.FromTimeSpan(arrivalTs),
                Departure: TimeOnly.FromTimeSpan(departureTs),
                Rl100: r.Rl100,
                Name: r.Name,
                Type: r.Type,
                Kbez: r.Kbez
            ));
        }

        return new ZugTimelineResult(zugNr, date, timeline);
    }
    public async Task<IReadOnlyList<ZwlMassnahmeOverlayDto>> GetOverlaysAsync(
        int      jahr,
        int      zugNr,
        DateOnly date) {
        const string sql = """
                           WITH params AS (
                               SELECT
                                   @Date::date AS d,
                                   @ZugNr      AS p_zug_nr,
                                   @Jahr       AS p_fahrplan_jahr
                           ),

                           base AS (
                               SELECT
                                   bb.rl100,
                                   COALESCE(nzvv.published_departure, nzvv.published_arrival) AS base_time
                               FROM ujbaudb.nfpl_zug nz
                               JOIN ujbaudb.nfpl_zug_variante nzv
                                   ON nzv.nfpl_zug_ref = nz.id
                               JOIN ujbaudb.nfpl_zug_variante_verlauf nzvv
                                   ON nzvv.nfpl_zug_var_ref = nzv.id
                               JOIN ujbaudb.basis_betriebsstelle bb
                                   ON nzvv.bst_ref = bb.id
                               CROSS JOIN params p
                               WHERE nz.zug_nr = p.p_zug_nr
                                 AND nz.fahrplan_jahr = p.p_fahrplan_jahr
                                 AND p.d BETWEEN nzvv.service_startdate AND nzvv.service_enddate
                                 AND SUBSTRING(
                                       nzvv.service_bitmask
                                       FROM ((p.d - nzvv.service_startdate) + 1)
                                       FOR 1
                                     ) = '1'
                                 AND COALESCE(nzvv.published_departure, nzvv.published_arrival) IS NOT NULL
                           ),

                           times AS (
                               SELECT
                                   b.*,
                                   LAG(base_time) OVER (ORDER BY base_time, rl100) AS prev_time,
                                   MAX(base_time) OVER () AS last_time
                               FROM base b
                           ),

                           gaps AS (
                               SELECT
                                   t.*,
                                   COALESCE(prev_time, last_time) AS prev_time_wrapped,
                                   CASE
                                       WHEN base_time >= COALESCE(prev_time, last_time)
                                           THEN base_time - COALESCE(prev_time, last_time)
                                       ELSE base_time - COALESCE(prev_time, last_time) + INTERVAL '24 hours'
                                   END AS forward_gap
                               FROM times t
                           ),

                           start_anchor AS (
                               SELECT base_time AS start_time
                               FROM gaps
                               ORDER BY forward_gap DESC
                               LIMIT 1
                           ),

                           timeline AS (
                               SELECT
                                   g.rl100,
                                   (
                                       (p.d + CASE WHEN g.base_time < a.start_time THEN 1 ELSE 0 END)
                                       + g.base_time
                                   ) AS event_ts
                               FROM gaps g
                               JOIN start_anchor a ON true
                               CROSS JOIN params p
                           ),

                           abschnitte AS (
                               SELECT
                                   rl100 AS von_rl100,
                                   LEAD(rl100) OVER (ORDER BY event_ts) AS bis_rl100,
                                   event_ts AS start_ts,
                                   LEAD(event_ts) OVER (ORDER BY event_ts) AS end_ts
                               FROM timeline
                           ),

                           massnahmen_roh AS (
                               SELECT
                                   a.von_rl100,
                                   a.bis_rl100,
                                   bmr.beginn  AS massnahme_beginn,
                                   bmr.ende    AS massnahme_ende,
                                   bsv.vzg_nr,
                                   bmr.regelung_kurz,
                                   bmr.durchgehend,
                                   bmr.zeitraum
                               FROM abschnitte a
                               JOIN ujbaudb.bbpneo_massnahme_regelung bmr
                                   ON tsrange(bmr.beginn, bmr.ende, '[]')
                                      && tsrange(a.start_ts, a.end_ts, '[]')
                                  AND bmr.aktiv
                               JOIN ujbaudb.basis_betriebsstelle2strecke bbsv
                                   ON bmr.bst2str_von_ref = bbsv.id
                               JOIN ujbaudb.basis_betriebsstelle2strecke bbsb
                                   ON bmr.bst2str_bis_ref = bbsb.id
                               JOIN ujbaudb.basis_betriebsstelle bbs
                                   ON bbsv.bst_ref = bbs.id
                               JOIN ujbaudb.basis_betriebsstelle bbb
                                   ON bbsb.bst_ref = bbb.id
                               JOIN ujbaudb.basis_strecke bsv
                                   ON bbsv.strecke_ref = bsv.id
                               WHERE a.bis_rl100 IS NOT NULL
                                 AND (
                                       (a.von_rl100 = bbs.rl100 AND a.bis_rl100 = bbb.rl100)
                                    OR (a.von_rl100 = bbb.rl100 AND a.bis_rl100 = bbs.rl100)
                                 )
                           )

                           SELECT
                               von_rl100     AS "VonRl100",
                               bis_rl100     AS "BisRl100",
                               massnahme_beginn AS "MassnahmeBeginn",
                               massnahme_ende   AS "MassnahmeEnde",

                               STRING_AGG(DISTINCT vzg_nr::text, ', '
                                          ORDER BY vzg_nr::text) AS "VzgListe",

                               STRING_AGG(DISTINCT regelung_kurz, ', '
                                          ORDER BY regelung_kurz) AS "Regelungen",

                               STRING_AGG(DISTINCT zeitraum, ', '
                                          ORDER BY zeitraum) AS "Zeitraum",

                               BOOL_OR(durchgehend) AS "Durchgehend"

                           FROM massnahmen_roh
                           GROUP BY
                               von_rl100,
                               bis_rl100,
                               massnahme_beginn,
                               massnahme_ende
                           ORDER BY massnahme_beginn;
                           """;
        
        await using var db   = await _dbFactory.CreateDbContextAsync();
        await using var conn = (NpgsqlConnection)db.Database.GetDbConnection();

        var result = await conn.QueryAsync<ZwlMassnahmeOverlayDto>(
            sql,
            new {
                Jahr  = jahr,
                ZugNr = zugNr,
                Date  = date.ToDateTime(TimeOnly.MinValue)
            });

        return result.ToList();
    }
}