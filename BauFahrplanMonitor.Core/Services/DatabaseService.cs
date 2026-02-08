using BauFahrplanMonitor.Core.Data;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace BauFahrplanMonitor.Core.Services;

/// <summary>
/// Zentraler Service für datenbankbezogene Infrastruktur-Aufgaben.
///
/// Verantwortlichkeiten:
/// <list type="bullet">
///   <item>Erzeugung neuer <see cref="UjBauDbContext"/>-Instanzen</item>
///   <item>Prüfung des Datenbankzustands (Schema-Version)</item>
/// </list>
///
/// Dieser Service enthält bewusst:
///  - keine Importlogik
///  - keine UI-Abhängigkeiten
/// </summary>
/// <remarks>
/// Architekturrolle:
/// <code>
/// Importer / UI / Startup
///   ↓
/// DatabaseService          ← HIER
///   ↓
/// IDbContextFactory
///   ↓
/// UjBauDbContext
/// </code>
///
/// Designentscheidungen:
/// <list type="bullet">
///   <item>Verwendung von <see cref="IDbContextFactory{TContext}"/></item>
///   <item>keine langlebigen DbContext-Instanzen</item>
///   <item>asynchrone Health-Checks</item>
/// </list>
/// </remarks>
public class DatabaseService {
    private static readonly Logger Log =
        LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Factory zur Erzeugung neuer DbContext-Instanzen.
    /// </summary>
    /// <remarks>
    /// Ermöglicht:
    ///  - thread-sichere Context-Erzeugung
    ///  - Nutzung außerhalb von Request-Scopes
    /// </remarks>
    private IDbContextFactory<UjBauDbContext> Factory { get; }

    /// <summary>
    /// Erstellt den DatabaseService.
    /// </summary>
    /// <param name="factory">
    /// DbContextFactory für <see cref="UjBauDbContext"/>.
    /// </param>
    public DatabaseService(IDbContextFactory<UjBauDbContext> factory) {
        Factory = factory;
        Log.Debug("📡 DatabaseService initialisiert (DbContextFactory aktiv).");
    }

    // ==========================================================
    // CONTEXT-ERZEUGUNG
    // ==========================================================

    /// <summary>
    /// Erstellt eine neue DbContext-Instanz.
    /// </summary>
    /// <remarks>
    /// Der Aufrufer ist für die Entsorgung
    /// (<c>Dispose</c> / <c>await using</c>) verantwortlich.
    /// </remarks>
    public UjBauDbContext CreateNewContext() {
        return Factory.CreateDbContext();
    }

    // ==========================================================
    // HEALTH-CHECK / SCHEMA
    // ==========================================================

    /// <summary>
    /// Mögliche Zustände der Datenbankprüfung.
    /// </summary>
    public enum DatabaseHealthStatus {
        Ok,
        Warning,
        Error
    }

    /// <summary>
    /// Ergebnis eines Datenbank-Health-Checks.
    /// </summary>
    /// <param name="Status">
    /// Ergebnisstatus (OK / Warning / Error).
    /// </param>
    /// <param name="Message">
    /// Menschlich lesbare Beschreibung.
    /// </param>
    /// <param name="CurrentSchemaVersion">
    /// Aktuell erkannte Schema-Version (falls verfügbar).
    /// </param>
    public record DatabaseCheckResult(
        DatabaseHealthStatus Status,
        string               Message,
        int?                 CurrentSchemaVersion
    );

    /// <summary>
    /// Prüft den aktuellen Zustand der Datenbank
    /// anhand der SchemaVersion-Tabelle.
    /// </summary>
    /// <param name="expectedSchema">
    /// Optional erwartete Schema-Version.
    /// </param>
    /// <returns>
    /// Ergebnis der Datenbankprüfung.
    /// </returns>
    /// <remarks>
    /// Ablauf:
    /// <list type="number">
    ///   <item>neueste SchemaVersion ermitteln</item>
    ///   <item>Vergleich mit erwartetem Wert</item>
    ///   <item>Status ableiten (OK / Warning / Error)</item>
    /// </list>
    ///
    /// Typische Einsatzorte:
    /// <list type="bullet">
    ///   <item>Application-Startup</item>
    ///   <item>Import-Vorprüfung</item>
    ///   <item>Admin-/Diagnose-UI</item>
    /// </list>
    /// </remarks>
    public async Task<DatabaseCheckResult> CheckDatabaseAsync(
        int? expectedSchema) {
        try {
            await using var ctx =
                await Factory.CreateDbContextAsync();

            // neueste Schema-Version bestimmen
            var entry = await ctx.SchemaVersion
                .OrderByDescending(x => x.UpdatedAt)
                .FirstOrDefaultAsync();

            if (entry == null) {
                return new DatabaseCheckResult(
                    DatabaseHealthStatus.Error,
                    "Keine Einträge in SchemaVersion",
                    null
                );
            }

            var current = (int)entry.Schema;

            // kein Erwartungswert definiert
            if (expectedSchema == null) {
                return new DatabaseCheckResult(
                    DatabaseHealthStatus.Ok,
                    $"Schema: {current} (kein Erwartungswert definiert)",
                    current
                );
            }

            // exakter Match
            if (expectedSchema == current) {
                return new DatabaseCheckResult(
                    DatabaseHealthStatus.Ok,
                    $"Schema OK: {current}",
                    current
                );
            }

            // Abweichung → Warnung
            return new DatabaseCheckResult(
                DatabaseHealthStatus.Warning,
                $"Schema: {current}, erwartet: {expectedSchema}",
                current
            );
        }
        catch (Exception ex) {
            Log.Error(ex.ToString());

            return new DatabaseCheckResult(
                DatabaseHealthStatus.Error,
                $"Fehler: {ex.Message}",
                null
            );
        }
    }
}