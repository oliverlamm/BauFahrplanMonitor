namespace BauFahrplanMonitor.Core.Jobs;

public sealed class ImportFileInfoDto {
    public string   FileName        { get; init; } = null!;
    public long     SizeBytes       { get; init; }
    public DateTime LastModifiedUtc { get; init; }
}