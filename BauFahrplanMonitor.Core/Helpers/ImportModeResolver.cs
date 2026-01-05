namespace BauFahrplanMonitor.Core.Helpers;

public static class ImportModeResolver {
    public static ImportMode Resolve(string filePath) {
        var name = Path.GetFileName(filePath).ToLowerInvariant();

        if (name.Contains("zvf"))
            return ImportMode.ZvF;

        if (name.Contains("üb") || name.Contains("ueb"))
            return ImportMode.UeB;

        return name.Contains("fplo") ? ImportMode.Fplo : ImportMode.None;

    }

    public static ImportMode ResolveFileType(string file) {
        var name = Path.GetFileName(file);
        if (string.IsNullOrWhiteSpace(name))
            return ImportMode.None;

        if (name.StartsWith("ZvF", StringComparison.OrdinalIgnoreCase))
            return ImportMode.ZvF;

        if (name.StartsWith("ÜB", StringComparison.OrdinalIgnoreCase))
            return ImportMode.UeB;

        if (name.StartsWith("Fplo", StringComparison.OrdinalIgnoreCase))
            return ImportMode.Fplo;

        return ImportMode.None;
    }

}