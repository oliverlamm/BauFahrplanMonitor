using System.Text.Json.Serialization;
using BauFahrplanMonitor.Core;
using BauFahrplanMonitor.Core.Jobs;
using BauFahrplanMonitor.Core.Tools;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Services;
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

app.MapPost("/api/import/zvfexport/import", (
    ZvFExportJob job) =>
{
    _ = Task.Run(() =>
        job.StartImportAsync(CancellationToken.None));

    return Results.Accepted(value: new {
        state = "importing"
    });
});


app.MapPost("/api/import/zvfexport/cancel", (
    ZvFExportJob job) => {
    job.Cancel();
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

app.MapPost("/api/import/zvfexport/scan", (
    ZvFScanRequest request,
    ZvFExportJob   job) => {
    // 🔑 Scan im Hintergrund starten
    _ = Task.Run(() =>
        job.StartScanAsync(
            request.Filter,
            CancellationToken.None));

    // 🔑 sofort zurückkehren
    return Results.Accepted(
        value: new {
            state  = "scanning",
            filter = request.Filter.ToString()
        });
});

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
            Importpfad         = cfg.Datei.Importpfad,
            Archivepfad        = cfg.Datei.Archivpfad,
            Archivieren        = cfg.Datei.Archivieren,
            NachImportLoeschen = cfg.Datei.NachImportLoeschen
        },

        allgemein = new {
            ImportThreads      = cfg.Allgemein.ImportThreads,
            debugging          = cfg.Allgemein.Debugging,
            stopAfterException = cfg.Allgemein.StopAfterException
        },

        zvfExport = new {
            status.State,
            status.TotalFiles,
            status.ProcessedFiles,
            status.Errors
        },

        endpoints = new[] {
            "/api/import/zvfexport/start", "/api/import/zvfexport/status", "/api/import/zvfexport/cancel"
        }
    });
});


app.Run();