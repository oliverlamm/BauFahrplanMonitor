using BauFahrplanMonitor.Core.Dto;

namespace BauFahrplanMonitor.Core.Services;

public sealed class ZugTimelineService {

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
}