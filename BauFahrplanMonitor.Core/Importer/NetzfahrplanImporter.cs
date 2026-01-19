using System.Diagnostics;
using BauFahrplanMonitor.Core.Helpers;
using BauFahrplanMonitor.Core.Importer.Helper;
using BauFahrplanMonitor.Core.Interfaces;
using BauFahrplanMonitor.Core.Services;
using BauFahrplanMonitor.Data;
using NLog;

namespace BauFahrplanMonitor.Core.Importer;

public sealed class NetzfahrplanImporter(
    IKssXmlLoader         xmlLoader,
    INetzfahrplanMapper   mapper,
    INetzfahrplanUpserter upserter,
    INfplZugCache         zugCache,
    ConfigService         configService)
    : IFileImporter {

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    // ----------------------------
    // ImportAsync
    // ----------------------------
    public async Task<ImportFileOutcome> ImportAsync(
        UjBauDbContext                db,
        ImportFileItem                item,
        IProgress<ImportProgressInfo> progress,
        CancellationToken             token) {

        ArgumentNullException.ThrowIfNull(item);
        token.ThrowIfCancellationRequested();

        if (!Path.GetFileName(item.FilePath)
                .StartsWith("KSS", StringComparison.OrdinalIgnoreCase)) {
            Logger.Info("Datei übersprungen (kein KSS): {0}", item.FilePath);

            return ImportFileOutcome.Skipped;
        }

        await ImportKssAsync(db, item.FilePath, progress, token);
        return ImportFileOutcome.Success;
    }

    // =====================================================================
    // KSS
    // =====================================================================
    private async Task ImportKssAsync(
        UjBauDbContext                 db,
        string                         filePath,
        IProgress<ImportProgressInfo>? progress,
        CancellationToken              token) {

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("FilePath ist leer.", nameof(filePath));

        var stopwatch = Stopwatch.StartNew();

        using (ScopeContext.PushProperty("ImportFile", Path.GetFileName(filePath))) {

            try {
                // -------------------------------------------------
                // 1) Start
                // -------------------------------------------------
                Report(progress, filePath, ImportSteps.Read, "Start");

                token.ThrowIfCancellationRequested();

                // -------------------------------------------------
                // 2) XML laden
                // -------------------------------------------------
                var kss = xmlLoader.Load(filePath);

                Report(progress, filePath, ImportSteps.Read, "XML geladen");

                token.ThrowIfCancellationRequested();

                // -------------------------------------------------
                // 3) Mapping (rein fachlich)
                // -------------------------------------------------
                var dto = mapper.Map(kss, filePath);

                Report(progress, filePath, ImportSteps.Map, "Mapping");

                if (Logger.IsDebugEnabled) {
                    Logger.Debug($"Netzfahrplan DTO: Züge={dto.Zuege.Count}");
                }

                token.ThrowIfCancellationRequested();

                // -------------------------------------------------
                // 4) Upsert (zugweise, threadsicher)
                // -------------------------------------------------
                Report(progress, filePath, ImportSteps.Upsert, "Upsert starten");

                IProgress<UpsertProgressInfo>? upsertProgress = null;

                if (progress != null) {
                    upsertProgress =
                        new Progress<UpsertProgressInfo>(p => {
                            var text = p.Phase switch {
                                UpsertPhase.Zuege =>
                                    $"Upsert Züge {p.Current}/{p.Total}",
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
                }

                await upserter.UpsertAsync(db, dto, zugCache, upsertProgress, token);

                token.ThrowIfCancellationRequested();

                // -------------------------------------------------
                // 5) Aufräumen
                // -------------------------------------------------
                Report(progress, filePath, ImportSteps.Cleanup, "Aufräumen");
                CleanupFile(filePath);

                // -------------------------------------------------
                // 6) Finalisieren
                // -------------------------------------------------
                Report(progress, filePath, ImportSteps.Finalize, "Abgeschlossen");

                stopwatch.Stop();

                Logger.Info(
                    "Netzfahrplan-Import abgeschlossen: Datei={Datei}, Dauer={Dauer}",
                    Path.GetFileName(filePath),
                    stopwatch.Elapsed);
            }
            catch (StopAfterExceptionException) {
                throw;
            }
            catch (OperationCanceledException) {
                Report(progress, filePath, ImportSteps.Read, "Abgebrochen");
                throw;
            }
            catch (Exception ex) {
                Logger.Error(ex, "Fehler beim Netzfahrplan-Import");
                Report(progress, filePath, ImportSteps.Read, ex.Message);
                throw;
            }
        }
    }

    // =====================================================================
    // Helper
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