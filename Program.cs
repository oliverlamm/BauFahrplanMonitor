using System;
using System.IO;
using Avalonia;
using BauFahrplanMonitor.Core.Importer;
using BauFahrplanMonitor.Core.Importer.Upsert;
using BauFahrplanMonitor.Core.Jobs;
using BauFahrplanMonitor.Core.Services;
using BauFahrplanMonitor.Core.Tools;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Importer;
using BauFahrplanMonitor.Importer.Helper;
using BauFahrplanMonitor.Importer.Mapper;
using BauFahrplanMonitor.Importer.Upsert;
using BauFahrplanMonitor.Importer.Xml;
using BauFahrplanMonitor.Interfaces;
using BauFahrplanMonitor.Resolver;
using BauFahrplanMonitor.Services;
using BauFahrplanMonitor.ViewModels;
using BauFahrplanMonitor.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NLog;

namespace BauFahrplanMonitor;

internal class Program {
    public static IServiceProvider Services { get; private set; } = null!;

    public static void Main(string[] args) {
        // NLog-Konfiguration laden
        var logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BauFahrplanMonitor",
            "logs");

        Directory.CreateDirectory(logDir);

        // Optional: nlog.config sicher aus BaseDirectory laden
        var nlogPath = Path.Combine(AppContext.BaseDirectory, "nlog.config");
        LogManager.Setup().LoadConfigurationFromFile(nlogPath);

        var log = LogManager.GetCurrentClassLogger();
        log.Info("BauFahrplanMonitor gestartet");

        var services = new ServiceCollection();

        // ======================================================
        // CONFIG / DB / INFRASTRUKTUR
        // ======================================================
        services.AddSingleton<ConfigService>();

        services.AddDbContextFactory<UjBauDbContext>((sp, options) => {
            var cfg = sp.GetRequiredService<ConfigService>();
            var cs  = cfg.BuildConnectionString();

            options.UseNpgsql(cs, npg => npg.UseNetTopologySuite());
        });

        services.AddSingleton<DatabaseService>();

        // ======================================================
        // UI / SERVICES
        // ======================================================
        services.AddSingleton<StatusMessageService>();
        services.AddSingleton<NavigationService>();
        services.AddSingleton<IFileDialogService, FileDialogService>();
        services.AddSingleton<StatusPageViewModel>();
        services.AddSingleton<StatusPageView>();
        services.AddSingleton<MainWindowViewModel>();

        // ======================================================
        // Shared Reference Resolver / Caches
        // ======================================================
        services.AddSingleton<SharedReferenceResolver>();
        services.AddSingleton<ZvFZugCache>();

        // ======================================================
        // Importer
        // ======================================================
        
        // ZvF Import
        services.AddSingleton<IZvFExportXmlLoader, ZvFExportXmlLoader>();
        services.AddSingleton<IZvFDtoMapper, ZvFDtoMapper>();
        services.AddSingleton<IZvFUpserter, ZvFUpserter>();

        // ÜB Import
        services.AddSingleton<IUeBUpserter, UeBUpserter>();
        services.AddSingleton<IUeBDtoMapper, UeBDtoMapper>();
        
        // Fplo Import
        services.AddSingleton<IFploUpserter, FploUpserter>();
        services.AddSingleton<IFploDtoMapper, FploDtoMapper>();

        // BBPNeo – Infrastruktur
        services.AddTransient<IBbpNeoXmlStreamingLoader, BbpNeoXmlStreamingLoader>();
        services.AddTransient<IBbpNeoUpserter, BbpNeoUpserter>();

        // BBPNeo – Importer selbst
        services.AddTransient<BbpNeoImporter>();
        
        // OsbBoB Import
        //services.AddTransient<OsbBobImporter>();
        
        // Netzfahrplan
        services.AddSingleton<IKssXmlLoader, KssXmlLoader>();
        services.AddSingleton<INetzfahrplanMapper, NetzfahrplanMapper>();
        services.AddSingleton<INetzfahrplanUpserter, NetzfahrplanUpserter>();
        services.AddSingleton<INfplZugCache, NfplZugCache>();
        
        // Importer selbst
        services.AddSingleton<NetzfahrplanImporter>();
        services.AddSingleton<ZvFExportImporter>();
        services.AddSingleton<IFileImporterFactory, FileImporterFactory>();

        // Job
        services.AddSingleton<ZvFExportJob>();
        services.AddSingleton<ImporterFacade>();

        services.AddSingleton<ZvFExportScanService>();
        
        Services = services.BuildServiceProvider(
            new ServiceProviderOptions {
                ValidateOnBuild = true,
                ValidateScopes  = true
            });


        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect();
}