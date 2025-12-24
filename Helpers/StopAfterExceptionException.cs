using System;

namespace BauFahrplanMonitor.Helpers;

public sealed class StopAfterExceptionException : Exception {
    public StopAfterExceptionException(string message)
        : base(message) {
    }
}