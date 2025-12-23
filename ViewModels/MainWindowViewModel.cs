using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BauFahrplanMonitor.Services;
using System.Collections.ObjectModel;
using BauFahrplanMonitor.Helpers;
using NLog;

namespace BauFahrplanMonitor.ViewModels;

public partial class MainWindowViewModel : ObservableObject {
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public                  string Title => "BauFahrplanMonitor";

    private readonly NavigationService    _navigationService;
    private readonly StatusMessageService _statusMessages;

    public NavigationService    Navigation     { get; }
    public StatusMessageService StatusMessages { get; }

    public ObservableCollection<string> Importer { get; } = [
        "Netzfahrplan",
        "BBPNeo",
        "ZvF/ÜB/Fplo",
        "OSB-Bob"
    ];

    public MainWindowViewModel(NavigationService navigationService, StatusMessageService statusMessages,
        NavigationService                        navigation) {
        _navigationService = navigationService;
        StatusMessages     = statusMessages;
        _statusMessages    = statusMessages;
        Navigation         = navigation;

        _statusMessages.Normal("Bereit");
        Logger.Info("Bereit");
        ShowStatus();
    }

    [RelayCommand]
    private void ShowStatus() {
        _navigationService.ShowStatus();
        _statusMessages.Normal("Systemstatus geöffnet");
        Logger.Info("Systemstatus geöffnet");
    }

    [RelayCommand]
    private void SelectImporter(string name) {
        StatusMessages.Normal($"Importer gewählt: {name}");
        Logger.Info($"Importer gewählt: {name}");

        switch (name) {
            case "Netzfahrplan":
                Navigation.ShowMultiFileImporter(
                    ImporterTyp.Direkt,
                    "Netzfahrplan Import"
                );
                break;

            case "ZvF/ÜB/Fplo":
                Navigation.ShowMultiFileImporter(
                    ImporterTyp.ZvFExport,
                    "ZvF / ÜB / Fplo Import"
                );
                break;

            default:
                StatusMessages.Warning($"Importer noch nicht implementiert: {name}");
                Logger.Warn($"Importer noch nicht implementiert: {name}");
                break;
        }
    }
}