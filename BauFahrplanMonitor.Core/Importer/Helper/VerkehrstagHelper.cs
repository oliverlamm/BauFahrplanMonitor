namespace BauFahrplanMonitor.Core.Importer.Helper;

public static class VerkehrstagHelper {
    /// <summary>
    /// Berechnet den effektiven Verkehrstag unter Berücksichtigung
    /// des Tagwechsel-Offsets (-1, 0, +1).
    /// </summary>
    public static DateOnly ApplyTagwechsel(
        DateOnly basisVerkehrstag,
        int      tagwechsel) {
        if (tagwechsel is < -2 or > 2)
            throw new ArgumentOutOfRangeException(
                nameof(tagwechsel),
                tagwechsel,
                "Tagwechsel muss -1, 0 oder 1 sein.");

        return basisVerkehrstag.AddDays(tagwechsel);
    }
}