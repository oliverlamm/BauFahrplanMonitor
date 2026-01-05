using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using BauFahrplanMonitor.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BauFahrplanMonitor.ViewModels;

public sealed class ImportWorkerViewModel : ObservableObject {
    public int WorkerIndex { get; }

    private ImportThreadStatus _status = ImportThreadStatus.Bereit;
    private DateTime?          _startTime;

    public void EnsureStarted() {
        _startTime ??= DateTime.UtcNow;
    }

    public TimeSpan? CalculateEta() {
        if (_startTime is null || Done <= 0 || Total <= 0)
            return null;

        var elapsed = DateTime.UtcNow - _startTime.Value;
        if (elapsed.TotalSeconds < 1)
            return null;

        var rate = Done / elapsed.TotalSeconds;
        if (rate <= 0)
            return null;

        var remaining = Total - Done;
        return TimeSpan.FromSeconds(remaining / rate);
    }

    public ImportThreadStatus Status {
        get => _status;
        set {
            if (!SetProperty(ref _status, value))
                return;
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusBrush));
        }
    }

    public string StatusText =>
        Status switch {
            ImportThreadStatus.Bereit               => "Bereit",
            ImportThreadStatus.Importieren          => "Importiere",
            ImportThreadStatus.Beendet              => "Beendet",
            ImportThreadStatus.Abbruch              => "Abbruch",
            ImportThreadStatus.Fehler               => "Fehler",
            ImportThreadStatus.AbbruchnachException => "Abbruch nach Exception",
            ImportThreadStatus.Abgeschlossen        => "Abgeschlossen",
            _                                       => "Unbekannt"
        };

    public IBrush StatusBrush =>
        Status switch {
            ImportThreadStatus.Bereit               => Brushes.Gray,
            ImportThreadStatus.Importieren          => Brushes.DodgerBlue,
            ImportThreadStatus.Abgeschlossen        => Brushes.ForestGreen,
            ImportThreadStatus.Fehler               => Brushes.IndianRed,
            ImportThreadStatus.Abbruch              => Brushes.DarkOrange,
            ImportThreadStatus.Beendet              => Brushes.DimGray,
            ImportThreadStatus.AbbruchnachException => Brushes.DarkRed,
            _                                       => Brushes.Gray
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
        Statistics.Add(new("Queue-Größe", "0"));
        Statistics.Add(new("Consumer aktiv", "0"));
        Statistics.Add(new("Consumer gesamt", "0"));
    }

    public void UpdateStat(string key, int value) {
        var stat = Statistics.FirstOrDefault(s => s.Key == key);
        if (stat != null)
        {
            stat.Value = value.ToString("N0");
        }
    }

    public void UpdateStat(string key, string value) {
        var stat = Statistics.FirstOrDefault(s => s.Key == key);
        if (stat != null) {
            stat.Value = value;
        }
    }

    public void ResetTiming() {
        _startTime = DateTime.UtcNow;
    }
    
    public bool IsFinal =>
        Status is ImportThreadStatus.Abbruch
            or ImportThreadStatus.AbbruchnachException
            or ImportThreadStatus.Abgeschlossen
            or ImportThreadStatus.Fehler;


    public DateTime? StartTime => _startTime;

}