using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using BauFahrplanMonitor.ViewModels;
using BauFahrplanMonitor.Views;
using System.Linq;
using NLog;

namespace BauFahrplanMonitor;

public class App : Application {
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted() {
        RequestedThemeVariant = ThemeVariant.Light; 

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            DisableAvaloniaDataAnnotationValidation();

            var mainVm = Program.Services.GetRequiredService<MainWindowViewModel>();
            Logger.Info("Anwendung gestartet");
            desktop.MainWindow = new MainWindow {
                DataContext = mainVm
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void DisableAvaloniaDataAnnotationValidation() {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators
                .OfType<DataAnnotationsValidationPlugin>()
                .ToArray();

        foreach (var plugin in dataValidationPluginsToRemove) {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}