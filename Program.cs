using System;
using System.IO;
using Avalonia;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Importer;
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
        services.AddSingleton<NetzfahrplanImporter>();

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
        
        // Importer selbst
        services.AddSingleton<ZvFExportImporter>();
        services.AddSingleton<IFileImporterFactory, FileImporterFactory>();

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