using BauFahrplanMonitor.Services;

namespace BauFahrplanMonitor.Helpers;

public sealed class StatusMessage {
    public StatusSeverity Severity { get; }
    public string         Text     { get; }

    public StatusMessage(StatusSeverity severity, string text) {
        Severity = severity;
        Text     = text;
    }
}