using System;
using BauFahrplanMonitor.Core.Helpers;

namespace BauFahrplanMonitor.Helpers;

public class ImportFileItem {
    public string     FilePath      { get; }
    public DateTime   SortTimestamp { get; }
    public ImportMode FileType      { get; }

    public ImportFileItem(string filePath, DateTime sortTimestamp, ImportMode importMode) {
        FilePath      = filePath;
        SortTimestamp = sortTimestamp;
        FileType      = importMode;
    }
}