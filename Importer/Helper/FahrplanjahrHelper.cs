using System;
using NLog;

namespace BauFahrplanMonitor.Importer.Helper;

public static class FahrplanjahrHelper {
    private static readonly Logger Log =
        LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Ermittelt das Fahrplanjahr anhand des Startdatums.
    ///
    /// Fahrplanjahr:
    /// - beginnt am 2. Sonntag im Dezember des Vorjahres
    /// - endet am 2. Samstag im Dezember des Jahres
    /// </summary>
    public static int? FromDateRange(DateOnly? start, DateOnly? end) {
        if (start == null) {
            Log.Debug(
                "[Fahrplanjahr] Kein Startdatum übergeben → kein Fahrplanjahr bestimmbar");
            return null;
        }

        Log.Debug(
            "[Fahrplanjahr] Bestimme Fahrplanjahr: Start={0}, End={1}",
            start,
            end);

        try {
            var result = FromStartDate(start.Value);

            Log.Debug(
                "[Fahrplanjahr] Ergebnis: Datum {0} → Fahrplanjahr {1}",
                start,
                result);

            return result;
        }
        catch (Exception ex) {
            Log.Error(
                ex,
                "[Fahrplanjahr] Fehler bei Bestimmung: Start={0}, End={1}",
                start,
                end);

            throw;
        }
    }

    /// <summary>
    /// Ermittelt das Fahrplanjahr anhand eines einzelnen Datums.
    /// </summary>
    private static int FromStartDate(DateOnly startDate) {
        var year = startDate.Year;

        Log.Debug(
            "[Fahrplanjahr] Prüfe Datum {0} gegen Fahrplanjahre {1} und {2}",
            startDate,
            year,
            year + 1);

        if (IsInFahrplanjahr(startDate, year)) {
            Log.Debug(
                "[Fahrplanjahr] Treffer: Datum {0} liegt im Fahrplanjahr {1}",
                startDate,
                year);
            return year;
        }

        if (IsInFahrplanjahr(startDate, year + 1)) {
            Log.Debug(
                "[Fahrplanjahr] Treffer: Datum {0} liegt im Fahrplanjahr {1}",
                startDate,
                year + 1);
            return year + 1;
        }

        // Das ist ein echter fachlicher Fehler
        Log.Error(
            "[Fahrplanjahr] Kein Fahrplanjahr gefunden für Datum {0}",
            startDate);

        throw new InvalidOperationException(
            $"Datum {startDate} konnte keinem Fahrplanjahr zugeordnet werden.");
    }

    /// <summary>
    /// Prüft, ob ein Datum innerhalb eines Fahrplanjahres liegt.
    /// </summary>
    private static bool IsInFahrplanjahr(DateOnly date, int fahrplanjahr) {
        var start = GetFahrplanjahrStart(fahrplanjahr);
        var end   = GetFahrplanjahrEnd(fahrplanjahr);

        var result = date >= start && date <= end;

        Log.Trace(
            "[Fahrplanjahr] Check: Datum={0}, Fahrplanjahr={1}, Von={2}, Bis={3} → {4}",
            date,
            fahrplanjahr,
            start,
            end,
            result);

        return result;
    }

    /// <summary>
    /// Start: 2. Sonntag im Dezember des Vorjahres.
    /// </summary>
    private static DateOnly GetFahrplanjahrStart(int fahrplanjahr) {
        var start = GetNthWeekdayOfMonth(
            year: fahrplanjahr - 1,
            month: 12,
            dayOfWeek: DayOfWeek.Sunday,
            n: 2);

        Log.Trace(
            "[Fahrplanjahr] Start Fahrplanjahr {0}: {1}",
            fahrplanjahr,
            start);

        return start;
    }

    /// <summary>
    /// Ende: 2. Samstag im Dezember des Fahrplanjahres.
    /// </summary>
    private static DateOnly GetFahrplanjahrEnd(int fahrplanjahr) {
        var end = GetNthWeekdayOfMonth(
            year: fahrplanjahr,
            month: 12,
            dayOfWeek: DayOfWeek.Saturday,
            n: 2);

        Log.Trace(
            "[Fahrplanjahr] Ende Fahrplanjahr {0}: {1}",
            fahrplanjahr,
            end);

        return end;
    }

    /// <summary>
    /// Liefert den n-ten Wochentag eines Monats.
    /// Beispiel: 2. Sonntag im Dezember.
    /// </summary>
    private static DateOnly GetNthWeekdayOfMonth(
        int       year,
        int       month,
        DayOfWeek dayOfWeek,
        int       n) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(n);

        var firstOfMonth = new DateOnly(year, month, 1);
        var offset =
            ((int)dayOfWeek - (int)firstOfMonth.DayOfWeek + 7) % 7;

        var day = 1 + offset + (n - 1) * 7;

        var result = new DateOnly(year, month, day);

        Log.Trace(
            "[Fahrplanjahr] {0}. {1} {2}/{3} → {4}",
            n,
            dayOfWeek,
            month,
            year,
            result);

        return result;
    }
}