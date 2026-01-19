using BauFahrplanMonitor.Core.Helpers;
using BauFahrplanMonitor.Core.Services;
using NLog;

namespace BauFahrplanMonitor.Core.Importer.Upsert;

/// <summary>
/// Zentrale Exception-Policy für alle Upserter
/// </summary>
public abstract class ImportUpserterBase(
    ConfigService config,
    Logger        logger) {
    protected readonly ConfigService Config = config;
    protected readonly Logger        Logger = logger;

    protected void HandleException(
        Exception ex,
        string    context,
        object?   details = null) {

        // ------------------------------------------------------------
        // 1) Cancellation ist KEIN fachlicher Fehler
        // ------------------------------------------------------------
        if (ex is OperationCanceledException) {
            Logger.Info(
                ex,
                "[Upsert:{Context}] Abgebrochen | Details={Details}",
                context,
                details);
            return;
        }

        // ------------------------------------------------------------
        // 2) Echte fachliche / technische Fehler
        // ------------------------------------------------------------
        Logger.Error(
            ex,
            "[Upsert:{Context}] Fachlicher Fehler | Details={Details}",
            context,
            details);

        // ------------------------------------------------------------
        // 3) Debug-Policy: StopAfterException
        // ------------------------------------------------------------
        if (Config.Effective.Allgemein is not { Debugging: true, StopAfterException: true })
            return;

        Logger.Fatal(
            "[Upsert:{Context}] StopAfterException aktiv → Import wird abgebrochen",
            context);

        throw new StopAfterExceptionException(
            "StopAfterException ausgelöst (Debug)");
    }
}