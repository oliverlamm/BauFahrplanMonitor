using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Helpers;
using BauFahrplanMonitor.Interfaces;
using BauFahrplanMonitor.Importer.Helper;
using BauFahrplanMonitor.Importer.Interface;
using BauFahrplanMonitor.Services;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace BauFahrplanMonitor.Importer;

public sealed class ZvFExportImporter(
    IZvFExportXmlLoader xmlLoader,
    IZvFDtoMapper       dtoMapper,
    IZvFUpserter        upserter,
    ConfigService       configService)
    : IFileImporter {
    private static readonly Logger Logger =
        LogManager.GetCurrentClassLogger();

    private readonly IZvFExportXmlLoader _xmlLoader =
        xmlLoader ?? throw new ArgumentNullException(nameof(xmlLoader));

    private readonly IZvFDtoMapper _dtoMapper =
        dtoMapper ?? throw new ArgumentNullException(nameof(dtoMapper));

    private readonly IZvFUpserter _upserter =
        upserter ?? throw new ArgumentNullException(nameof(upserter));

    // =====================================================================
    // ENTRYPOINT (Interface)
    // =====================================================================

    public async Task ImportAsync(
        UjBauDbContext                db,
        ImportFileItem                item,
        IProgress<ImportProgressInfo> progress,
        CancellationToken             token) {
        ArgumentNullException.ThrowIfNull(item);
        token.ThrowIfCancellationRequested();

        switch (item.FileType) {
            case ImportMode.ZvF:
                await ImportZvFAsync(db, item.FilePath, progress, token);
                break;

            case ImportMode.UeB:
                await ImportUeBAsync(item.FilePath, token);
                break;

            case ImportMode.Fplo:
                await ImportFploAsync(item.FilePath, token);
                break;

            default:
                throw new InvalidOperationException(
                    $"Unbekannter FileType: {item.FileType}");
        }
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
                Report(progress, filePath, ImportSteps.Read, "XML laden");

                token.ThrowIfCancellationRequested();

                // -------------------------------------------------
                // 2) Mapping
                // -------------------------------------------------
                var dto = _dtoMapper.Map(zvfExport, filePath);
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

                token.ThrowIfCancellationRequested();

                // -------------------------------------------------
                // 3) Merge
                // -------------------------------------------------
                ZvFMergeHelper.MergeZuege(dto.Document);

                Logger.Debug(
                    "ZvF Merge abgeschlossen: ZuegeRaw={0}, Zuege={1}",
                    dto.Document.ZuegeRaw.Count,
                    dto.Document.Zuege.Count);

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
                        Step       = text,
                        StepIndex  = ImportSteps.Upsert,
                        TotalSteps = ImportSteps.TotalSteps,
                        SubIndex   = p.Current,
                        SubTotal   = p.Total
                    });
                });

                var result      = await _upserter.UpsertAsync(db, dto, upsertProgress, token);
                var dokumentRef = result.DokumentRef;
                var stats       = result.Stats;

                // -------------------------------------------------
                // 5) Aufräumen
                // -------------------------------------------------
                Report(progress, filePath, ImportSteps.Cleanup, "Aufräumen");
                CleanupFile(filePath);

                // -------------------------------------------------
                // 6) Finalisieren
                // -------------------------------------------------
                await _upserter.MarkImportCompletedAsync(dokumentRef, token);
                Report(progress, filePath, ImportSteps.Finalize, "Abgeschlossen");

                stopwatch.Stop();

                Logger.Info(
                    "ZvF-Import abgeschlossen: Datei='{0}', Dauer={1:mm\\:ss\\.fff}, {2}",
                    Path.GetFileName(filePath),
                    stopwatch.Elapsed,
                    stats);
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

    private Task ImportUeBAsync(string filePath, CancellationToken token) =>
        throw new NotImplementedException("ÜB-Importer noch nicht implementiert.");

    private Task ImportFploAsync(string filePath, CancellationToken token) =>
        throw new NotImplementedException("FPLO-Importer noch nicht implementiert.");

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
            Step       = stepText,
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