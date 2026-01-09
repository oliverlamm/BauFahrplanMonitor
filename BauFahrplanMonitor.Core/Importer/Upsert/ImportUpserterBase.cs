using BauFahrplanMonitor.Core.Services;
using BauFahrplanMonitor.Helpers;
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
        Logger.Error(ex, $"[Upsert:{context}] Fachlicher Fehler | Details={details}");

        if (Config.Effective.Allgemein is not { Debugging: true, StopAfterException: true }) return;
        Logger.Fatal($"[Upsert:{context}] StopAfterException aktiv → Import wird abgebrochen");

        throw new StopAfterExceptionException("StopAfterException ausgelöst (Debug)");
    }
}