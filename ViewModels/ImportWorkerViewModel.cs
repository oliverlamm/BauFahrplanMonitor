using System.Collections.ObjectModel;
using System.Linq;
using BauFahrplanMonitor.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BauFahrplanMonitor.ViewModels;

public sealed class ImportWorkerViewModel : ObservableObject {
    public int WorkerIndex { get; }

    private ImportThreadStatus _status = ImportThreadStatus.Bereit;

    public ImportThreadStatus Status {
        get => _status;
        set {
            if (SetProperty(ref _status, value))
                OnPropertyChanged(nameof(StatusText));
        }
    }

    public string StatusText =>
        Status switch {
            ImportThreadStatus.Bereit      => "Bereit",
            ImportThreadStatus.Importieren => "Importiere",
            ImportThreadStatus.Beendet     => "Beendet",
            ImportThreadStatus.Abbruch     => "Abbruch",
            ImportThreadStatus.Fehler      => "Fehler",
            _                              => "Unbekannt"
        };

    private int _done;

    public int Done {
        get => _done;
        set {
            if (SetProperty(ref _done, value))
                OnPropertyChanged(nameof(ProgressText));
        }
    }

    private int _total;

    public int Total {
        get => _total;
        set {
            if (SetProperty(ref _total, value))
                OnPropertyChanged(nameof(ProgressText));
        }
    }

    public string ProgressText =>
        $"{Done:N0} / {Total:N0} Datensätze";

    public ObservableCollection<StatisticItemViewModel> Statistics { get; }
        = new();

    public ImportWorkerViewModel(int index) {
        WorkerIndex = index;

        Statistics.Add(new("Anzahl Maßnahmen", "0"));
        Statistics.Add(new("Anzahl Regelungen", "0"));
        Statistics.Add(new("Anzahl BvE", "0"));
        Statistics.Add(new("Anzahl APS", "0"));
        Statistics.Add(new("Anzahl IAV", "0"));
    }

    public void UpdateStat(string key, int value) {
        var stat = Statistics.FirstOrDefault(s => s.Key == key);
        if (stat != null)
            stat.Value = value.ToString("N0");
    }
}