namespace BauFahrplanMonitor.Core.Helpers;

public sealed class StopAfterExceptionException : Exception {
    public StopAfterExceptionException(string message)
        : base(message) {
    }
}