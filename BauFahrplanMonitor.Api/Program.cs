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

app.MapGet("/api/import/zvfexport/status", (
    ZvFExportJob job) => {
    return Results.Ok(job.GetStatus());
});

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
app.MapGet("/", (
    ConfigService  config,
    ImporterFacade facade) => {
    var status = facade.GetZvFExportStatus();
    var cfg    = config.Effective;

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

        datei = new {
            cfg.Datei.Importpfad,
            cfg.Datei.Archivpfad,
            cfg.Datei.Archivieren,
            cfg.Datei.NachImportLoeschen
        },

        allgemein = new {
            cfg.Allgemein.ImportThreads,
            cfg.Allgemein.Debugging,
            cfg.Allgemein.StopAfterException
        },

        zvfExport = new {
            status.State,
            status.TotalFiles,
            status.ProcessedFiles,
            status.Errors
        },

        endpoints = new[] {
            "/api/import/zvfexport/start", 
            "/api/import/zvfexport/status", 
            "/api/import/zvfexport/cancel",
            "/api/import/netzfahrplan/start",
            "/api/import/netzfahrplan/status",
            "/api/import/netzfahrplan/cancel",
            "/api/import/bbpneo/start",
            "/api/import/bbpneo/status",
            "/api/import/bbpneo/cancel",
        }
    });
});


app.Run();