using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using BauFahrplanMonitor.Core.Data;
using BauFahrplanMonitor.Core.Helpers;
using BauFahrplanMonitor.Core.Importer.Helper;
using BauFahrplanMonitor.Core.Interfaces;
using BauFahrplanMonitor.Core.Services;
using NLog;

namespace BauFahrplanMonitor.Core.Importer;

public sealed class ZvFExportImporter(
    IZvFExportXmlLoader xmlLoader,
    IZvFDtoMapper       zvFDtoMapper,
    IZvFUpserter        zvFUpserter,
    IUeBDtoMapper       ueBDtoMapper,
    IUeBUpserter        ueBUpserter,
    IFploDtoMapper      fploDtoMapper,
    IFploUpserter       fploUpserter,
    ConfigService       configService)
    : IFileImporter {
    private static readonly Logger Logger =
        LogManager.GetCurrentClassLogger();

    private readonly IZvFExportXmlLoader _xmlLoader =
        xmlLoader ?? throw new ArgumentNullException(nameof(xmlLoader));

    private readonly IZvFDtoMapper _zvFDtoMapper =
        zvFDtoMapper ?? throw new ArgumentNullException(nameof(zvFDtoMapper));

    private readonly IUeBDtoMapper _ueBDtoMapper =
        ueBDtoMapper ?? throw new ArgumentNullException(nameof(ueBDtoMapper));

    private readonly IFploDtoMapper _fploDtoMapper =
        fploDtoMapper ?? throw new ArgumentNullException(nameof(fploDtoMapper));

    private readonly IZvFUpserter _zvFupserter =
        zvFUpserter ?? throw new ArgumentNullException(nameof(zvFUpserter));

    private readonly IUeBUpserter _ueBUpserter =
        ueBUpserter ?? throw new ArgumentNullException(nameof(ueBUpserter));

    private readonly IFploUpserter _fploUpserter =
        fploUpserter ?? throw new ArgumentNullException(nameof(fploUpserter));

    // =====================================================================
    // ENTRYPOINT (Interface)
    // =====================================================================

    public async Task<ImportFileOutcome> ImportAsync(
        UjBauDbContext                db,
        ImportFileItem                item,
        IProgress<ImportProgressInfo> progress,
        CancellationToken             token) {
        ArgumentNullException.ThrowIfNull(item);
        token.ThrowIfCancellationRequested();

        try {
            switch (item.FileType) {
                case ImportMode.ZvF:
                    await ImportZvFAsync(db, item.FilePath, progress, token);
                    break;

                case ImportMode.UeB:
                    await ImportUeBAsync(db, item.FilePath, progress, token);
                    break;

                case ImportMode.Fplo:
                    await ImportFploAsync(db, item.FilePath, progress, token);
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unbekannter FileType: {item.FileType}");
            }
        }
        catch (InvalidOperationException ex)
            when (ex.Message.Contains("error in XML document", StringComparison.OrdinalIgnoreCase)) {
            // --------------------------------------------------
            // 🔑 XML kaputt → Datei überspringen
            // --------------------------------------------------
            Logger.Error(
                ex,
                "Ungültiges XML – Datei wird übersprungen: {File}",
                item.FilePath);

            progress?.Report(new ImportProgressInfo {
                FileName  = Path.GetFileName(item.FilePath),
                StepText  = "Ungültiges XML – Datei übersprungen",
                StepIndex = ImportSteps.Read
            });

            return ImportFileOutcome.Skipped;
        }
        return ImportFileOutcome.Success;
    }

    // =====================================================================
    // ZVF
    // =====================================================================

    private async Task ImportZvFAsync(
        UjBauDbContext                db,
        string                        filePath,
        IProgress<ImportProgressInfo> progress,
        CancellationToken             token) {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("FilePath ist leer.", nameof(filePath));

        var stopwatch = Stopwatch.StartNew();

        using (ScopeContext.PushProperty(
                   "ImportFile",
                   Path.GetFileName(filePath))) {
            try {
                Report(progress, filePath, ImportSteps.Read, "Start");

                token.ThrowIfCancellationRequested();

                // -------------------------------------------------
                // 1) XML laden
                // -------------------------------------------------
                var zvfExport = _xmlLoader.Load(filePath);
                var readDone = stopwatch.Elapsed;
                Report(progress, filePath, ImportSteps.Read, "XML laden");

                token.ThrowIfCancellationRequested();

                // -------------------------------------------------
                // 2) Mapping
                // -------------------------------------------------
                var dto = _zvFDtoMapper.Map(zvfExport, filePath);
                Report(progress, filePath, ImportSteps.Map, "Mapping");

                if (Logger.IsDebugEnabled) {
                    var json = JsonSerializer.Serialize(
                        dto,
                        new JsonSerializerOptions {
                            WriteIndented          = true,
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder
                                .UnsafeRelaxedJsonEscaping
                        });

                    Logger.Debug(
                        "ZvF DTO nach Mapping:{0}{1}",
                        Environment.NewLine,
                        json);
                }

                var mapDone = stopwatch.Elapsed;

                token.ThrowIfCancellationRequested();

                // -------------------------------------------------
                // 3) Merge
                // -------------------------------------------------
                ZvFMergeHelper.MergeZuege(dto.Document);

                Logger.Debug(
                    "ZvF Merge abgeschlossen: ZuegeRaw={0}, Zuege={1}",
                    dto.Document.ZuegeRaw.Count,
                    dto.Document.Zuege.Count);

                var preparationDone = stopwatch.Elapsed;
                Report(progress, filePath, ImportSteps.Merge, "Züge mergen");

                token.ThrowIfCancellationRequested();

                // -------------------------------------------------
                // 4) Upsert
                // -------------------------------------------------
                Report(progress, filePath, ImportSteps.Upsert, "Upsert starten");

                var upsertProgress = new Progress<UpsertProgressInfo>(p => {
                    var text = p.Phase switch {
                        UpsertPhase.Zuege =>
                            $"Upsert Züge {p.Current}/{p.Total}",

                        UpsertPhase.Entfallen =>
                            $"Upsert Entfallen {p.Current}/{p.Total}",

                        _ => "Upsert"
                    };

                    progress.Report(new ImportProgressInfo {
                        FileName   = Path.GetFileName(filePath),
                        StepText   = text,
                        StepIndex  = ImportSteps.Upsert,
                        TotalSteps = ImportSteps.TotalSteps,
                        SubIndex   = p.Current,
                        SubTotal   = p.Total
                    });
                });

                var result      = await _zvFupserter.UpsertAsync(db, dto, upsertProgress, token);
                var upsertDone  = stopwatch.Elapsed;
                var dokumentRef = result.DokumentRef;
                var stats       = result.ZvFStats;

                // -------------------------------------------------
                // 5) Aufräumen
                // -------------------------------------------------
                Report(progress, filePath, ImportSteps.Cleanup, "Aufräumen");
                CleanupFile(filePath);

                // -------------------------------------------------
                // 6) Finalisieren
                // -------------------------------------------------
                await _zvFupserter.MarkImportCompletedAsync(dokumentRef, token);
                LogPhaseDurations("ZvF", filePath, readDone, mapDone, preparationDone, upsertDone, stopwatch.Elapsed);
                Report(progress, filePath, ImportSteps.Finalize, "Abgeschlossen");

                stopwatch.Stop();

                Logger.Info(
                    "ZvF-Import abgeschlossen: Datei={Datei}, Dauer={Dauer}, {Summary}",
                    dto.Document.Dateiname,
                    stopwatch.Elapsed,
                    stats.ToSummaryString());
            }
            catch (StopAfterExceptionException) {
                // 🔑 kontrollierter Abbruch → NICHT als Fehler loggen
                throw;
            }
            catch (OperationCanceledException) {
                Report(progress, filePath, ImportSteps.Read, "Abgebrochen");
                throw;
            }
            catch (Exception ex) {
                Logger.Error(ex, "Fehler beim ZvF-Import");
                Report(progress, filePath, ImportSteps.Read, ex.Message);
                throw;
            }
        }
    }

    // =====================================================================
    // ÜB / FPLO (noch nicht implementiert)
    // =====================================================================

    private async Task ImportUeBAsync(
        UjBauDbContext                db,
        string                        filePath,
        IProgress<ImportProgressInfo> progress,
        CancellationToken             token) {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("FilePath ist leer.", nameof(filePath));

        var stopwatch = Stopwatch.StartNew();

        using (ScopeContext.PushProperty("ImportFile", Path.GetFileName(filePath))) {
            try {
                Report(progress, filePath, ImportSteps.Read, "Start");

                token.ThrowIfCancellationRequested();

                // -------------------------------------------------
                // 1) XML laden
                // -------------------------------------------------
                var uebExport = _xmlLoader.Load(filePath);
                var readDone = stopwatch.Elapsed;
                Report(progress, filePath, ImportSteps.Read, "XML laden");

                token.ThrowIfCancellationRequested();

                // -------------------------------------------------
                // 2) Mapping
                // -------------------------------------------------
                var dto = _ueBDtoMapper.Map(uebExport, filePath);
                Report(progress, filePath, ImportSteps.Map, "Mapping");

                if (Logger.IsDebugEnabled) {
                    var json = JsonSerializer.Serialize(
                        dto,
#pragma warning disable CA1869
                        new JsonSerializerOptions {
#pragma warning restore CA1869
                            WriteIndented          = true,
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder
                                .UnsafeRelaxedJsonEscaping
                        });

                    Logger.Debug("ÜB DTO nach Mapping:{0}{1}", Environment.NewLine, json);
                }

                var mapDone = stopwatch.Elapsed;

                token.ThrowIfCancellationRequested();

                // -------------------------------------------------
                // 3) Merge (ÜB: Platzhalter)
                // -------------------------------------------------
                dto.Document.Zuege.Clear();
                dto.Document.Zuege.AddRange(dto.Document.ZuegeRaw);

                var preparationDone = stopwatch.Elapsed;
                Report(progress, filePath, ImportSteps.Merge, "Vorverarbeitung");

                token.ThrowIfCancellationRequested();

                // -------------------------------------------------
                // 4) Upsert
                // -------------------------------------------------
                Report(progress, filePath, ImportSteps.Upsert, "Upsert starten");

                var upsertProgress =
                    new Progress<UpsertProgressInfo>(p => {
                        var text = p.Phase switch {
                            UpsertPhase.Zuege =>
                                $"Upsert Züge {p.Current}/{p.Total}",

                            UpsertPhase.Sev =>
                                $"Upsert SEV {p.Current}/{p.Total}",

                            _ => "Upsert"
                        };

                        progress.Report(new ImportProgressInfo {
                            FileName   = Path.GetFileName(filePath),
                            StepText   = text,
                            StepIndex  = ImportSteps.Upsert,
                            TotalSteps = ImportSteps.TotalSteps,
                            SubIndex   = p.Current,
                            SubTotal   = p.Total
                        });
                    });

                var result =
                    await _ueBUpserter.UpsertAsync(
                        db,
                        dto,
                        upsertProgress,
                        token);
                var upsertDone = stopwatch.Elapsed;

                var dokumentRef = result.DokumentRef;
                var stats       = result.UeBStats;

                // -------------------------------------------------
                // 5) Aufräumen
                // -------------------------------------------------
                Report(progress, filePath, ImportSteps.Cleanup, "Aufräumen");
                CleanupFile(filePath);

                // -------------------------------------------------
                // 6) Finalisieren
                // -------------------------------------------------
                await _ueBUpserter.MarkImportCompletedAsync(
                    dokumentRef,
                    token);
                LogPhaseDurations("ÜB", filePath, readDone, mapDone, preparationDone, upsertDone, stopwatch.Elapsed);

                Report(progress, filePath, ImportSteps.Finalize, "Abgeschlossen");

                stopwatch.Stop();

                Logger.Info(
                    "ÜB-Import abgeschlossen: Datei={Datei}, Dauer={Dauer}, {Summary}",
                    dto.Document.Dateiname,
                    stopwatch.Elapsed,
                    stats.ToSummaryString());
            }
            catch (StopAfterExceptionException) {
                // 🔑 kontrollierter Abbruch → NICHT als Fehler loggen
                throw;
            }
            catch (OperationCanceledException) {
                Report(progress, filePath, ImportSteps.Read, "Abgebrochen");
                throw;
            }
            catch (Exception ex) {
                Logger.Error(ex, "Fehler beim ÜB-Import");
                Report(progress, filePath, ImportSteps.Read, ex.Message);
                throw;
            }
        }
    }

    private async Task ImportFploAsync(
        UjBauDbContext                db,
        string                        filePath,
        IProgress<ImportProgressInfo> progress,
        CancellationToken             token) {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("FilePath ist leer.", nameof(filePath));

        var stopwatch = Stopwatch.StartNew();

        using (ScopeContext.PushProperty("ImportFile", Path.GetFileName(filePath))) {
            try {
                Report(progress, filePath, ImportSteps.Read, "Start");

                token.ThrowIfCancellationRequested();

                // -------------------------------------------------
                // 1) XML laden
                // -------------------------------------------------
                var uebExport = _xmlLoader.Load(filePath);
                var readDone = stopwatch.Elapsed;
                Report(progress, filePath, ImportSteps.Read, "XML laden");

                token.ThrowIfCancellationRequested();

                // -------------------------------------------------
                // 2) Mapping
                // -------------------------------------------------
                var dto = _fploDtoMapper.Map(uebExport, filePath);
                Report(progress, filePath, ImportSteps.Map, "Mapping");

                if (Logger.IsDebugEnabled) {
                    var json = JsonSerializer.Serialize(
                        dto,
#pragma warning disable CA1869
                        new JsonSerializerOptions {
#pragma warning restore CA1869
                            WriteIndented          = true,
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder
                                .UnsafeRelaxedJsonEscaping
                        });

                    Logger.Debug(
                        "Fplo DTO nach Mapping:{0}{1}",
                        Environment.NewLine,
                        json);
                }

                var mapDone = stopwatch.Elapsed;

                token.ThrowIfCancellationRequested();

                // -------------------------------------------------
                // 3) Merge (Fplo: Platzhalter)
                // -------------------------------------------------
                dto.Document.Zuege.Clear();
                dto.Document.Zuege.AddRange(dto.Document.ZuegeRaw);

                var preparationDone = stopwatch.Elapsed;
                Report(progress, filePath, ImportSteps.Merge, "Vorverarbeitung");

                token.ThrowIfCancellationRequested();

                // -------------------------------------------------
                // 4) Upsert
                // -------------------------------------------------
                Report(progress, filePath, ImportSteps.Upsert, "Upsert starten");

                var upsertProgress =
                    new Progress<UpsertProgressInfo>(p => {
                        var text = p.Phase switch {
                            UpsertPhase.Zuege =>
                                $"Upsert Züge {p.Current}/{p.Total}",

                            UpsertPhase.Sev =>
                                $"Upsert SEV {p.Current}/{p.Total}",

                            _ => "Upsert"
                        };

                        progress.Report(new ImportProgressInfo {
                            FileName   = Path.GetFileName(filePath),
                            StepText   = text,
                            StepIndex  = ImportSteps.Upsert,
                            TotalSteps = ImportSteps.TotalSteps,
                            SubIndex   = p.Current,
                            SubTotal   = p.Total
                        });
                    });

                var result =
                    await _fploUpserter.UpsertAsync(
                        db,
                        dto,
                        upsertProgress,
                        token);
                var upsertDone = stopwatch.Elapsed;

                var dokumentRef = result.DokumentRef;
                var stats       = result.FploStats;

                // -------------------------------------------------
                // 5) Aufräumen
                // -------------------------------------------------
                Report(progress, filePath, ImportSteps.Cleanup, "Aufräumen");
                CleanupFile(filePath);

                // -------------------------------------------------
                // 6) Finalisieren
                // -------------------------------------------------
                await _fploUpserter.MarkImportCompletedAsync(
                    dokumentRef,
                    token);
                LogPhaseDurations("FPLO", filePath, readDone, mapDone, preparationDone, upsertDone, stopwatch.Elapsed);

                Report(progress, filePath, ImportSteps.Finalize, "Abgeschlossen");

                stopwatch.Stop();

                Logger.Info(
                    "Fplo-Import abgeschlossen: Datei={Datei}, Dauer={Dauer}, {Summary}",
                    dto.Document.Dateiname,
                    stopwatch.Elapsed,
                    stats.ToSummaryString());
            }
            catch (StopAfterExceptionException) {
                // 🔑 kontrollierter Abbruch → NICHT als Fehler loggen
                throw;
            }
            catch (OperationCanceledException) {
                Report(progress, filePath, ImportSteps.Read, "Abgebrochen");
                throw;
            }
            catch (Exception ex) {
                Logger.Error(ex, "Fehler beim Fplo-Import");
                Report(progress, filePath, ImportSteps.Read, ex.Message);
                throw;
            }
        }
    }

    // =====================================================================
    // Per-file timing; does not include queueing before the importer starts.
    // =====================================================================
    private static void LogPhaseDurations(
        string format,
        string filePath,
        TimeSpan readDone,
        TimeSpan mapDone,
        TimeSpan preparationDone,
        TimeSpan upsertDone,
        TimeSpan finished) {
        Logger.Info(
            "Importphasen: Format={Format}, Datei={Datei}, LesenMs={LesenMs}, MappingMs={MappingMs}, VorbereitenMs={VorbereitenMs}, UpsertMs={UpsertMs}, AbschlussMs={AbschlussMs}",
            format,
            Path.GetFileName(filePath),
            Math.Round(readDone.TotalMilliseconds),
            Math.Round((mapDone - readDone).TotalMilliseconds),
            Math.Round((preparationDone - mapDone).TotalMilliseconds),
            Math.Round((upsertDone - preparationDone).TotalMilliseconds),
            Math.Round((finished - upsertDone).TotalMilliseconds));
    }

    // =====================================================================
    // Progress-Helper
    // =====================================================================

    private static void Report(
        IProgress<ImportProgressInfo>? progress,
        string                         file,
        int                            stepIndex,
        string                         stepText) {
        progress?.Report(new ImportProgressInfo {
            FileName   = Path.GetFileName(file),
            StepText   = stepText,
            StepIndex  = stepIndex,
            TotalSteps = ImportSteps.TotalSteps
        });
    }

    // =====================================================================
    // Cleanup
    // =====================================================================

    private void CleanupFile(string filePath) {
        if (configService.Effective.Datei is
            { Archivieren: false, NachImportLoeschen: false })
            return;

        if (configService.Effective.Datei.Archivieren) {
            var target = Path.Combine(
                configService.Effective.Datei.Archivpfad,
                Path.GetFileName(filePath));

            Directory.CreateDirectory(
                configService.Effective.Datei.Archivpfad);

            File.Copy(filePath, target, overwrite: true);
        }

        if (configService.Effective.Datei.NachImportLoeschen) {
            File.Delete(filePath);
        }
    }
}