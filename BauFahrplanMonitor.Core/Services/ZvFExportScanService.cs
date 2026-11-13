using System.Xml;
using BauFahrplanMonitor.Core.Data;
using BauFahrplanMonitor.Core.Helpers;
using BauFahrplanMonitor.Core.Importer.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BauFahrplanMonitor.Core.Services;

public sealed class ZvFExportScanService(
    ConfigService                     config,
    IDbContextFactory<UjBauDbContext> dbFactory,
    ILogger<ZvFExportScanService>     logger) {

    // 🔁 exakt wie früher
    private Dictionary<string, ImportDbInfo> _dbImportCache =
        new(StringComparer.OrdinalIgnoreCase);

    private List<string> ScanFilesZvFExport() {
        var root = config.Effective.Datei.Importpfad;

        return Directory
            .EnumerateFiles(root, "*.xml", SearchOption.AllDirectories)
            .ToList();
    }

    private Task BuildImportCacheAsync() {
        logger.LogInformation("Lade Import-Cache aus der Datenbank…");

        var cache = new Dictionary<string, ImportDbInfo>(
            StringComparer.OrdinalIgnoreCase);

        using var db = dbFactory.CreateDbContext();

        foreach (var d in GetZvfDokumente(db))
            if (!string.IsNullOrWhiteSpace(d.FileName))
                cache[d.FileName] = d;

        foreach (var d in GetUebDokumente(db))
            if (!string.IsNullOrWhiteSpace(d.FileName))
                cache[d.FileName] = d;

        foreach (var d in GetFploDokumente(db))
            if (!string.IsNullOrWhiteSpace(d.FileName))
                cache[d.FileName] = d;

        _dbImportCache = cache;

        logger.LogInformation(
            "Import-Cache geladen ({Count} Einträge)",
            cache.Count);

        return Task.CompletedTask;
    }

    private List<ImportDbInfo> GetZvfDokumente(UjBauDbContext db) =>
        db.ZvfDokument
            .Select(s => new ImportDbInfo {
                FileName        = s.Dateiname,
                ExportTimestamp = s.ExportTimestamp,
                ImportTimestamp = s.ImportTimestamp
            })
            .ToList();

    private List<ImportDbInfo> GetUebDokumente(UjBauDbContext db) =>
        db.UebDokument
            .Select(s => new ImportDbInfo {
                FileName        = s.Dateiname,
                ExportTimestamp = s.ExportTimestamp,
                ImportTimestamp = s.ImportTimestamp
            })
            .ToList();

    private List<ImportDbInfo> GetFploDokumente(UjBauDbContext db) =>
        db.FploDokument
            .Select(s => new ImportDbInfo {
                FileName        = s.Dateiname,
                ExportTimestamp = s.ExportTimestamp,
                ImportTimestamp = s.ImportTimestamp
            })
            .ToList();

    private bool ShouldImport(string filePath) {
        var name = Path.GetFileName(filePath);
        return !_dbImportCache.ContainsKey(name);
    }
    
    private ImportFileItem? TryCreateImportItem(string file) {
        try {
            var ts = ReadTimestampFromHeader(file);

            return new ImportFileItem(
                file,
                ts ?? DateTime.UtcNow,
                ImportModeResolver.Resolve(file));
        }
        catch (Exception ex) {
            logger.LogError(ex, "Fehler beim Erstellen des ImportItems: {File}", file);
            return null;
        }
    }

    private DateTime? ReadTimestampFromHeader(string filePath) {
        try {
            using var stream = File.OpenRead(filePath);
            using var reader = XmlReader.Create(stream, new XmlReaderSettings {
                IgnoreComments   = true,
                IgnoreWhitespace = true,
                DtdProcessing    = DtdProcessing.Prohibit,
                XmlResolver      = null
            });

            // Only the first timestamp is needed. Avoid materializing the
            // complete document during the import scan.
            while (reader.Read()) {
                if (reader.NodeType != XmlNodeType.Element ||
                    reader.LocalName != "Timestamp")
                    continue;

                var value = reader.ReadElementContentAsString();
                if (DateTime.TryParse(value, out var parsed))
                    return parsed;

                break;
            }
        }
        catch (Exception ex) {
            logger.LogDebug(ex, "Timestamp nicht aus XML-Header lesbar: {File}", filePath);
        }

        return File.GetCreationTimeUtc(filePath);
    }

    private static bool IsAllowed(ImportMode mode, ZvFFileFilter filter) {
        return filter switch {
            ZvFFileFilter.All  => mode is ImportMode.ZvF or ImportMode.UeB or ImportMode.Fplo,
            ZvFFileFilter.ZvF  => mode == ImportMode.ZvF,
            ZvFFileFilter.UeB  => mode == ImportMode.UeB,
            ZvFFileFilter.Fplo => mode == ImportMode.Fplo,
            _                  => false
        };
    }

    private static void CountNew(ScanStat stat, ImportMode mode) {
        switch (mode) {
            case ImportMode.ZvF: stat.ZvF_New++; break;
            case ImportMode.UeB: stat.UeB_New++; break;
            case ImportMode.Fplo: stat.Fplo_New++; break;
            case ImportMode.Kss: stat.Kss_New++; break;
        }
    }

    private static void CountImported(ScanStat stat, ImportMode mode) {
        switch (mode) {
            case ImportMode.ZvF: stat.ZvF_Imported++; break;
            case ImportMode.UeB: stat.UeB_Imported++; break;
            case ImportMode.Fplo: stat.Fplo_Imported++; break;
            case ImportMode.Kss: stat.Kss_Imported++; break;
        }
    }

    public IReadOnlyList<ScanCandidate> PreScan(
        ZvFFileFilter     filter,
        CancellationToken token) {
        var files  = ScanFilesZvFExport();
        var result = new List<ScanCandidate>(files.Count);

        foreach (var file in files) {
            token.ThrowIfCancellationRequested();

            logger.LogDebug("PreScan: {File}", file);

            var mode = ImportModeResolver.ResolveFileType(file);
            if (mode == ImportMode.None)
                continue;

            if (!IsAllowed(mode, filter))
                continue;

            result.Add(new ScanCandidate {
                FilePath = file,
                Mode     = mode
            });
        }

        logger.LogInformation("PreScan abgeschlossen | Kandidaten={Count}", result.Count);

        return result;
    }


    public async Task<IReadOnlyList<ImportFileItem>> ValidateAsync(
        IReadOnlyList<ScanCandidate> candidates,
        ScanStat                     stat,
        IProgress<string>?           progress,
        CancellationToken            token) {
        logger.LogInformation(
            "Validierungs-Scan gestartet | Kandidaten={Count}",
            candidates.Count);

        await BuildImportCacheAsync();

        var queue = new List<ImportFileItem>();

        foreach (var c in candidates) {
            token.ThrowIfCancellationRequested();

            // 🔑 Fortschritt: genau EIN Tick pro geprüfter Datei
            progress?.Report(c.FilePath);

            using (logger.BeginScope(new Dictionary<string, object> {
                       ["ImportFile"] = c.FilePath
                   })) {

                logger.LogDebug(
                    "Validate gestartet | PreScanMode={Mode}",
                    c.Mode);

                // Check the database cache before opening or parsing XML.
                // The pre-scan already identified the import mode.
                if (!ShouldImport(c.FilePath)) {
                    logger.LogDebug("Übersprungen (bereits importiert)");
                    CountImported(stat, c.Mode);
                    continue;
                }

                var item = TryCreateImportItem(c.FilePath);
                if (item == null) {
                    logger.LogDebug("Übersprungen (kein ImportItem erzeugt)");
                    continue;
                }

                logger.LogDebug(
                    "Header gelesen | DetectedMode={Mode}",
                    item.FileType);

                logger.LogDebug("Neu für Import");
                CountNew(stat, item.FileType);
                queue.Add(item);
            }
        }

        logger.LogInformation(
            "Validierungs-Scan abgeschlossen | Queue={Queue} | ZvF={ZvF_New}/{ZvF_Imported} | ÜB={UeB_New}/{UeB_Imported} | Fplo={Fplo_New}/{Fplo_Imported}",
            queue.Count,
            stat.ZvF_New, stat.ZvF_Imported,
            stat.UeB_New, stat.UeB_Imported,
            stat.Fplo_New, stat.Fplo_Imported);

        return queue;
    }
}