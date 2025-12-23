using System;
using BauFahrplanMonitor.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace BauFahrplanMonitor.Data;

/* Wichtig: Einzufügen nach Scafolding:
 * partial void OnCreated();
   partial void OnConfiguringPartial(DbContextOptionsBuilder optionsBuilder);
 */

/// <summary>
/// Partieller DbContext für ujBauDB.
///
/// Dieser Partial erweitert den generierten DbContext
/// um:
///  - Zugriff auf den <see cref="ConfigService"/>
///  - sichere Nachkonfiguration bei manueller Erzeugung
///  - optionales EF-Core-SQL-Logging über NLog
/// </summary>
/// <remarks>
/// Architekturziel:
/// <list type="bullet">
///   <item>Ein DbContext für Runtime, Tools und Tests</item>
///   <item>Keine doppelte Konfiguration bei DI-Nutzung</item>
///   <item>Konfigurierbares EF-Core-Logging</item>
/// </list>
///
/// Wichtig:
/// <para>
/// Diese Klasse ergänzt bewusst nur Teile von
/// <c>OnCreated</c> und <c>OnConfiguring</c>
/// über Partial-Methoden.
/// </para>
/// </remarks>
public partial class UjBauDbContext {

    /// <summary>
    /// Referenz auf den globalen ConfigService.
    /// </summary>
    /// <remarks>
    /// Wird lazy aus der App-DI geladen,
    /// da der DbContext auch außerhalb der
    /// normalen DI-Pipeline erzeugt werden kann
    /// (z. B. Migrationstools, Tests, CLI).
    /// </remarks>
    private ConfigService? _config;

    /*
     * Ergänzt:
     *  partial void OnCreated();
     *  partial void OnConfiguringPartial(DbContextOptionsBuilder optionsBuilder);
     */

    // ==========================================================
    // ON CREATED
    // ==========================================================

    /// <summary>
    /// Wird aufgerufen, unabhängig davon,
    /// ob der DbContext über DI oder manuell erzeugt wurde.
    /// </summary>
    /// <remarks>
    /// Zweck:
    ///  - Zugriff auf den <see cref="ConfigService"/> ermöglichen
    ///  - robuste Initialisierung auch außerhalb von DI
    ///
    /// Fehler beim Laden der Config
    /// führen NICHT zum Abbruch,
    /// sondern werden geloggt.
    /// </remarks>
    partial void OnCreated() {
        try {
            _config ??= Program.Services.GetService<ConfigService>();
        }
        catch (Exception ex) {
            var log = LogManager.GetCurrentClassLogger();
            log.Error(
                ex,
                "UjBauDbContext.OnCreated: Konnte ConfigService nicht laden");
        }
    }

    // ==========================================================
    // ON CONFIGURING (PARTIAL)
    // ==========================================================

    /// <summary>
    /// Ergänzt die OnConfiguring-Logik nur dann,
    /// wenn der DbContext NICHT bereits konfiguriert ist.
    /// </summary>
    /// <param name="optionsBuilder">
    /// EF Core OptionsBuilder
    /// </param>
    /// <remarks>
    /// Diese Methode wird nur aktiv:
    /// <list type="bullet">
    ///   <item>bei manueller Erzeugung des DbContext</item>
    ///   <item>bei Tools (z. B. Migrationen)</item>
    /// </list>
    ///
    /// Wird der DbContext über DI oder eine Factory erzeugt,
    /// ist <see cref="DbContextOptionsBuilder.IsConfigured"/>
    /// bereits <c>true</c> → keine Doppelkonfiguration.
    /// </remarks>
    partial void OnConfiguringPartial(
        DbContextOptionsBuilder optionsBuilder) {

        // ------------------------------------------------------
        // Guard: bereits konfiguriert → nichts tun
        // ------------------------------------------------------
        if (optionsBuilder.IsConfigured)
            return;

        if (_config == null)
            return;

        var db = _config.Effective.Datenbank;
        ArgumentNullException.ThrowIfNull(db);

        // ------------------------------------------------------
        // Connection String bauen
        // ------------------------------------------------------
        var cs =
            $"Host={db.Host};" +
            $"Port={db.Port};" +
            $"Database={db.Database};" +
            $"Username={db.User};" +
            $"Password={db.Password}";

        // ------------------------------------------------------
        // EF Core konfigurieren
        // ------------------------------------------------------
        var efLogger = LogManager.GetLogger("EFCore.SQL");

        optionsBuilder.UseNpgsql(
            cs,
            o => o.UseNetTopologySuite());

        // ------------------------------------------------------
        // Optionales SQL-Logging
        // ------------------------------------------------------
        if (db.EFLogging == true) {
            optionsBuilder
                .LogTo(
                    efLogger.Debug,
                    [DbLoggerCategory.Database.Command.Name],
                    LogLevel.Information)
                .EnableDetailedErrors();
        }

        // ------------------------------------------------------
        // Sensitive Data Logging (optional!)
        // ------------------------------------------------------
        if (db.EFSensitiveLogging == true)
            optionsBuilder.EnableSensitiveDataLogging();
    }
}
