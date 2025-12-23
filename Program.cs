using System;
using System.IO;
using Avalonia;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Importer;
using BauFahrplanMonitor.Importer.Interface;
using BauFahrplanMonitor.Importer.Mapper;
using BauFahrplanMonitor.Importer.Upsert;
using BauFahrplanMonitor.Importer.Xml;
using BauFahrplanMonitor.Resolver;
using Microsoft.Extensions.DependencyInjection;
using BauFahrplanMonitor.Services;
using BauFahrplanMonitor.ViewModels;
using BauFahrplanMonitor.Views;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace BauFahrplanMonitor;

internal class Program {
    public static IServiceProvider Services { get; private set; } = null!;

    public static void Main(string[] args) {
        // NLog-Konfiguration laden
        var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BauFahrplanMonitor", "logs");

        if (!Directory.Exists(logDir)) {
            Directory.CreateDirectory(logDir);
        }

        LogManager.Setup()
            .LoadConfigurationFromFile("nlog.config");

        var log = LogManager.GetCurrentClassLogger();
        log.Info("BauFahrplanMonitor gestartet");

        var services = new ServiceCollection();

        // Registrierung von ConfigService und DatabaseService
        services.AddSingleton<ConfigService>();   // Registriere ConfigService
        
        // ======================================================
        // DB / INFRASTRUKTUR
        // ======================================================
        var configService = new ConfigService();
        services.AddDbContextFactory<UjBauDbContext>(options => {
            var cs = configService.BuildConnectionString();
            options.UseNpgsql(cs, npg => npg.UseNetTopologySuite());
        });

        services.AddSingleton<DatabaseService>();

        // Registrierung des StatusMessageService
        services.AddSingleton<StatusMessageService>();  // Registriere StatusMessageService

        // Weitere ViewModels und Views
        services.AddSingleton<StatusPageViewModel>();
        services.AddSingleton<StatusPageView>();
        services.AddSingleton<NavigationService>();
        services.AddSingleton<MainWindowViewModel>();
        
        // --------------------------------------------------
        // Shared Reference Resolver
        // --------------------------------------------------
        services.AddSingleton<SharedReferenceResolver>();
        services.AddDbContextFactory<UjBauDbContext>();

        services.AddSingleton<ZvFZugCache>(); 

        
        services.AddSingleton<NetzfahrplanImporter>();
        
        // --------------------------------------------------
        // ZvF Import – Core
        // --------------------------------------------------
        services.AddSingleton<IZvFExportXmlLoader, ZvFExportXmlLoader>();
        services.AddSingleton<IZvFDtoMapper, ZvFDtoMapper>();
        services.AddSingleton<IZvFUpserter, ZvFUpserter>();

        // --------------------------------------------------
        // Importer selbst
        // --------------------------------------------------
        services.AddSingleton<ZvFExportImporter>();


        services.AddSingleton<IFileImporterFactory, FileImporterFactory>();
        
        // Erstelle den ServiceProvider
        var serviceProvider = services.BuildServiceProvider();
        Services = serviceProvider;

        // Starte die Avalonia App
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }


    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect();
}