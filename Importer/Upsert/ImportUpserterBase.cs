using System;
using BauFahrplanMonitor.Helpers;
using BauFahrplanMonitor.Services;
using NLog;

namespace BauFahrplanMonitor.Importer.Upsert;

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