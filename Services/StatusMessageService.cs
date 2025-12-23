using BauFahrplanMonitor.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BauFahrplanMonitor.Services;

public sealed class StatusMessageService : ObservableObject {
    private StatusMessage _current =
        new(StatusSeverity.Normal, "Bereit");

    public StatusMessage Current {
        get => _current;
        private set => SetProperty(ref _current, value);
    }

    // ---------------- API ----------------

    public void Normal(string text)
        => Set(StatusSeverity.Normal, text);

    public void Warning(string text)
        => Set(StatusSeverity.Warning, text);

    public void Error(string text)
        => Set(StatusSeverity.Error, text);

    public void Success(string text)
        => Set(StatusSeverity.Success, text);

    private void Set(StatusSeverity severity, string text) {
        Current = new StatusMessage(severity, text);
    }
}