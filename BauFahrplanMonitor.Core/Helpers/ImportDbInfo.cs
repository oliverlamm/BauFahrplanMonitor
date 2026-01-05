using System;

namespace BauFahrplanMonitor.Helpers;

public sealed class ImportDbInfo {
    public string?    FileName        { get; init; } = string.Empty;
    public DateTime?  ExportTimestamp { get; init; }
    public DateTime? ImportTimestamp { get; init; }
}