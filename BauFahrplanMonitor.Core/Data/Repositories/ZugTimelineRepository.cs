using System.Data;
using BauFahrplanMonitor.Core.Dto;
using Dapper;

namespace BauFahrplanMonitor.Core.Data.Repositories;

public sealed class ZugTimelineRepository : IZugTimelineRepository {

    private readonly IDbConnection _db;

    public ZugTimelineRepository(IDbConnection db) {
        _db = db;
    }

    public async Task<IReadOnlyList<RawZugTimelineRow>> LoadAsync(
        int               zugNr,
        DateOnly          date,
        CancellationToken ct
    ) {
        var sqlDate = date.ToDateTime(TimeOnly.MinValue);

        const string sql = """
                           WITH base AS (
                               SELECT
                                   nz.zug_nr,
                                   nzv.kind,
                                   bb.rl100,
                                   bb.name,
                                   bbt.kbez,
                                   bbt.bezeichner,
                                   nzvv.type,
                                   nzvv.published_arrival,
                                   nzvv.published_departure,
                                   CASE
                                       WHEN nzvv.published_departure IS NOT NULL
                                            AND nzvv.published_departure <> TIME '00:00:00'
                                           THEN nzvv.published_departure
                                       ELSE nzvv.published_arrival
                                   END AS base_time
                               FROM ujbaudb.nfpl_zug nz
                               JOIN ujbaudb.nfpl_zug_variante nzv
                                 ON nzv.nfpl_zug_ref = nz.id
                               JOIN ujbaudb.nfpl_zug_variante_verlauf nzvv
                                 ON nzvv.nfpl_zug_var_ref = nzv.id
                               JOIN ujbaudb.basis_betriebsstelle bb
                                 ON nzvv.bst_ref = bb.id
                               JOIN ujbaudb.basis_betriebsstelle_typ bbt
                                 ON bb.typ_ref = bbt.id
                               WHERE nz.zug_nr = @zugNr
                                 AND nz.fahrplan_jahr = @year
                                 AND @date::date BETWEEN nzvv.service_startdate AND nzvv.service_enddate
                                 AND SUBSTRING(
                                       nzvv.service_bitmask
                                       FROM (@date::date - nzvv.service_startdate + 1)
                                       FOR 1
                                     ) = '1'
                                 AND nzvv.published_departure <> TIME '00:00:00'
                                 AND COALESCE(bbt.kbez, '') NOT IN ('Bk', '#N/A')
                           ),
                           times AS (
                               SELECT
                                   b.*,
                                   LAG(base_time) OVER (ORDER BY base_time) AS prev_time,
                                   MAX(base_time) OVER ()                   AS last_time
                               FROM base b
                           ),
                           gaps AS (
                               SELECT
                                   t.*,
                                   COALESCE(prev_time, last_time) AS prev_time_wrapped,
                                   CASE
                                       WHEN base_time >= COALESCE(prev_time, last_time)
                                           THEN (base_time - COALESCE(prev_time, last_time))
                                       ELSE (base_time - COALESCE(prev_time, last_time) + INTERVAL '24 hours')
                                   END AS forward_gap
                               FROM times t
                           ),
                           start_anchor AS (
                               SELECT base_time AS start_time
                               FROM gaps
                               ORDER BY forward_gap DESC, base_time
                               LIMIT 1
                           ),
                           timeline AS (
                               SELECT
                                   g.*,
                                   a.start_time,
                                   CASE WHEN g.base_time < a.start_time THEN 1 ELSE 0 END AS day_offset,
                                   ( @date::date
                                     + (CASE WHEN g.base_time < a.start_time THEN 1 ELSE 0 END)
                                     + g.base_time
                                   ) AS sort_dt
                               FROM gaps g
                               JOIN start_anchor a ON true
                           )
                           SELECT
                               ROW_NUMBER() OVER (
                                   ORDER BY sort_dt, CASE type WHEN 'stop' THEN 0 ELSE 1 END
                               ) AS SeqNo,
                               zug_nr           AS ZugNr,
                               kind,
                               rl100,
                               name,
                               kbez,
                               type,
                               published_arrival   AS PublishedArrival,
                               published_departure AS PublishedDeparture,
                               day_offset          AS DayOffset,
                               sort_dt             AS SortDt
                           FROM timeline
                           ORDER BY
                               sort_dt,
                               CASE type WHEN 'stop' THEN 0 ELSE 1 END;
                           """;

        var rows = await _db.QueryAsync<RawZugTimelineRow>(
            new CommandDefinition(
                sql,
                new {
                    zugNr,
                    year = date.Year,
                    date = sqlDate
                },
                cancellationToken: ct
            )
        );

        return rows.ToList();
    }
}