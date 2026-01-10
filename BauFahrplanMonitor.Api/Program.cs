using System.Text.Json.Serialization;
using BauFahrplanMonitor.Core;
using BauFahrplanMonitor.Core.Jobs;
using BauFahrplanMonitor.Core.Services;
using BauFahrplanMonitor.Core.Tools;
using BauFahrplanMonitor.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Extensions.Logging;

var builder  = WebApplication.CreateBuilder(args);
var services = builder.Services;

// ---------------- NLog ----------------
var logger = LogManager.Setup()
    .LoadConfigurationFromFile("nlog.config", optional: false)
    .GetCurrentClassLogger();

builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
builder.Logging.AddNLog();

// ------------- JSON -------------------
services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.IncludeFields = true;
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter());
});


// ----------- DB -----------------------
services.AddDbContextFactory<UjBauDbContext>((sp, options) => {
    var cfg = sp.GetRequiredService<ConfigService>();
    var cs  = cfg.BuildConnectionString();
    options.UseNpgsql(cs, npg => npg.UseNetTopologySuite());
});

// -------- Importer --------------------
services.AddImporterServices();

// --------------------------------------
var app = builder.Build();

// --------------------------------------
// ZvFExport Importer
// --------------------------------------
app.MapPost("/api/import/zvfexport/import", (
    ZvFExportJob job) => {
    _ = Task.Run(() =>
        job.StartImportAsync(CancellationToken.None));

    return Results.Accepted(value: new {
        state = "importing"
    });
});


app.MapPost("/api/import/zvfexport/cancel", (
    ZvFExportJob job) => {
    job.RequestCancel();
    return Results.Accepted(
        value: new {
            state = "cancelled"
        }
    );
});

app.MapGet("/api/import/zvfexport/status", (ZvFExportJob job) => { return Results.Ok(job.Status); });


app.MapPost("/api/import/zvfexport/scan", async (
    ZvFScanRequest request,
    ZvFExportJob   job) => {
    await job.TriggerScanAsync(
        request.Filter,
        CancellationToken.None);

    return Results.Accepted(
        value: new {
            state  = "scanning",
            filter = request.Filter.ToString()
        });
});

// --------------------------------------
// Netzfahrplan Importer
// --------------------------------------
app.MapPost("/api/import/netzfahrplan/scan",
    async (NetzfahrplanJob job) => {
        await job.ScanAsync(CancellationToken.None);
        return Results.Accepted();
    });

app.MapPost("/api/import/netzfahrplan/start",
    async (NetzfahrplanJob job) => {
        await job.StartImportAsync(CancellationToken.None);
        return Results.Accepted();
    });

app.MapPost("/api/import/netzfahrplan/cancel",
    (NetzfahrplanJob job) => {
        job.RequestSoftCancel();
        return Results.Accepted();
    });

app.MapGet("/api/import/netzfahrplan/status",
    (NetzfahrplanJob job) => job.Status);

// --------------------------------------
// BBPNeo Importer
// --------------------------------------
app.MapPost("/api/import/bbpneo/start",
    async (
        BbpNeoImportRequest      request,
        [FromServices] BbpNeoJob job,
        CancellationToken        token) => {
        if (string.IsNullOrWhiteSpace(request.FilePath))
            return Results.BadRequest("FilePath fehlt");

        await job.StartAsync(request.FilePath, token);

        return Results.Accepted(
            value: new {
                state = "started",
                file  = request.FilePath
            });
    });

app.MapPost("/api/import/bbpneo/cancel",
    ([FromServices] BbpNeoJob job) => {
        job.RequestCancel();
        return Results.Accepted(
            value: new {
                state = "cancel-requested"
            });
    });

app.MapGet("/api/import/bbpneo/status",
    ([FromServices] BbpNeoJob job) =>
        Results.Ok(job.Status));

// --------------------------------------
// Status
// --------------------------------------
app.MapGet("/api/status",
    async (
        [FromServices] ConfigService   config,
        [FromServices] ImporterFacade  facade,
        [FromServices] DatabaseService databaseService) => {

        var cfg = config.Effective;

        // =================================================
        // Datenbankstatus
        // =================================================
        var expectedSchema =
            cfg.Datenbank.ExpectedSchemaVersion;

        DatabaseService.DatabaseCheckResult? dbResult = null;
        string                               dbMessage;

        try {
            dbResult =
                await databaseService.CheckDatabaseAsync(expectedSchema);

            dbMessage = dbResult.Status switch {
                DatabaseService.DatabaseHealthStatus.Ok =>
                    $"Schema OK: {dbResult.CurrentSchemaVersion}",

                DatabaseService.DatabaseHealthStatus.Warning =>
                    $"Schema: {dbResult.CurrentSchemaVersion}, erwartet: {expectedSchema}",

                DatabaseService.DatabaseHealthStatus.Error =>
                    "Datenbankfehler",

                _ => "Unbekannter Status"
            };
        }
        catch (Exception ex) {
            dbMessage = $"Fehler: {ex.Message}";
        }

        // =================================================
        // Datei-Pfade (Import / Archiv)
        // =================================================
        var importPath  = cfg.Datei.Importpfad;
        var archivePath = cfg.Datei.Archivpfad;

        string importStatus;
        string importMessage;

        if (!Directory.Exists(importPath)) {
            importStatus  = "Error";
            importMessage = "Importpfad existiert nicht";
        }
        else {
            importStatus  = "Ok";
            importMessage = "Importpfad existiert";
        }

        string archiveStatus;
        string archiveMessage;

        if (!Directory.Exists(archivePath)) {
            archiveStatus  = "Error";
            archiveMessage = "Archivpfad existiert nicht";
        }
        else if (!CanWriteToDirectory(archivePath)) {
            archiveStatus = "Warning";
            archiveMessage =
                "Archivpfad existiert, ist aber nicht beschreibbar";
        }
        else {
            archiveStatus = "Ok";
            archiveMessage =
                "Archivpfad existiert und ist beschreibbar";
        }

        // =================================================
        // Importer-Status (ZvFExport)
        // =================================================
        var zvfStatus = facade.GetZvFExportStatus();

        // =================================================
        // Antwort
        // =================================================
        return Results.Ok(new {
            application = "BauFahrplanMonitor.Api",
            session     = config.SessionKey,
            name        = cfg.System.Name,
            version     = cfg.System.Version,
            time        = DateTime.UtcNow,

            datenbank = new {
                cfg.Datenbank.Host,
                cfg.Datenbank.Port,
                cfg.Datenbank.Database,
                cfg.Datenbank.User,
                cfg.Datenbank.EFLogging,
                cfg.Datenbank.EFSensitiveLogging,
                cfg.Datenbank.ExpectedSchemaVersion,
            },

            databaseStatus = new {
                status = dbResult?.Status
                         ?? DatabaseService.DatabaseHealthStatus.Error,

                currentSchemaVersion =
                    dbResult?.CurrentSchemaVersion,

                expectedSchemaVersion = expectedSchema,

                message = dbMessage
            },

            paths = new {
                import = new {
                    path    = importPath,
                    status  = importStatus,
                    message = importMessage
                },
                archive = new {
                    path    = archivePath,
                    status  = archiveStatus,
                    message = archiveMessage
                }
            },

            datei = new {
                cfg.Datei.Importpfad,
                cfg.Datei.Archivpfad,
                cfg.Datei.Archivieren,
                cfg.Datei.NachImportLoeschen
            },

            allgemein = new {
                cfg.Allgemein.ImportThreads,
                cfg.Allgemein.Debugging,
                cfg.Allgemein.StopAfterException,
                cfg.System.Name,
                cfg.System.Version,
                Environment.MachineName
            }
        });
    });


app.Run();
return;

static bool CanWriteToDirectory(string path) {
    try {
        var testFile = Path.Combine(
            path,
            $".write_test_{Guid.NewGuid():N}");

        using (File.Create(testFile)) {
        }

        File.Delete(testFile);

        return true;
    }
    catch {
        return false;
    }
}