using Avalonia.Media;
using BauFahrplanMonitor.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BauFahrplanMonitor.ViewModels;

public class ImportThreadViewModel : ObservableObject {
    public int ThreadIndex { get; }

    // -------------------------------------------------
    // Status
    // -------------------------------------------------
    private ImportThreadStatus _status;

    public ImportThreadStatus Status {
        get => _status;
        set {
            if (!SetProperty(ref _status, value)) return;
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusBrush));
        }
    }

    public string StatusText => Status.ToString();

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

    // -------------------------------------------------
    // Datei / Statusmeldung
    // -------------------------------------------------
    private string _fileName = string.Empty;

    public string FileName {
        get => _fileName;
        set => SetProperty(ref _fileName, value);
    }

    private string _statusMessage = string.Empty;

    /// <summary>
    /// Textuelle Beschreibung des aktuellen Schritts
    /// z. B. "Upsert Züge"
    /// </summary>
    public string StatusMessage {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    // -------------------------------------------------
    // Fortschritt (NUR Schritte innerhalb einer Datei)
    // -------------------------------------------------
    private int _threadProgress;

    /// <summary>
    /// Fortschritt der aktuellen Datei in Prozent (0–100),
    /// abgeleitet aus StepIndex / TotalSteps
    /// </summary>
    public int ThreadProgress {
        get => _threadProgress;
        set => SetProperty(ref _threadProgress, value);
    }

    private string _threadProgressText = string.Empty;

    /// <summary>
    /// Schrittanzeige, z. B.:
    ///  - "3 / 8 Schritte"
    ///  - "Upsert Züge 17 / 300"
    /// </summary>
    public string ThreadProgressText {
        get => _threadProgressText;
        set => SetProperty(ref _threadProgressText, value);
    }

    // -------------------------------------------------
    // ctor
    // -------------------------------------------------
    public ImportThreadViewModel(int index) {
        ThreadIndex        = index;
        Status             = ImportThreadStatus.Bereit;
        FileName           = string.Empty;
        StatusMessage      = "Warte auf Start";
        ThreadProgress     = 0;
        ThreadProgressText = string.Empty;
    }
}