using System;
using BauFahrplanMonitor.Helpers;
using BauFahrplanMonitor.ViewModels;
using BauFahrplanMonitor.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace BauFahrplanMonitor.Services;

public class NavigationService : ObservableObject {
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private          object?          _currentViewModel;
    private readonly IServiceProvider _provider;

    public NavigationService(IServiceProvider provider) {
        _provider = provider;
    }

    public object? CurrentViewModel {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    public void ShowStatus() {
        var vm = _provider.GetRequiredService<StatusPageViewModel>();
        CurrentViewModel = new StatusPageView { DataContext = vm };
    }

    // 🔽 NEU
    public void ShowMultiFileImporter(ImporterTyp importerTyp, string title) {
        Logger.Info($"Navigiere zu MultiFileImporter: {importerTyp}, {title}");
        var vm = ActivatorUtilities.CreateInstance<MultiFileImporterViewModel>(
            _provider,
            importerTyp,
            title
        );

        var view = new MultiFileImporterView {
            DataContext = vm
        };

        CurrentViewModel = view;
    }
}