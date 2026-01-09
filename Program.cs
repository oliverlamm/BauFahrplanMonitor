using System;
using System.IO;
using Avalonia;
using BauFahrplanMonitor.Core;
using BauFahrplanMonitor.Core.Importer;
using BauFahrplanMonitor.Core.Importer.Helper;
using BauFahrplanMonitor.Core.Importer.Mapper;
using BauFahrplanMonitor.Core.Importer.Upsert;
using BauFahrplanMonitor.Core.Importer.Xml;
using BauFahrplanMonitor.Core.Interfaces;
using BauFahrplanMonitor.Core.Jobs;
using BauFahrplanMonitor.Core.Resolver;
using BauFahrplanMonitor.Core.Services;
using BauFahrplanMonitor.Core.Tools;
using BauFahrplanMonitor.Data;
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
        services.AddImporterServices();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect();
}