namespace BauFahrplanMonitor.Importer.Helper;

public sealed class ZvFUpsertResult {
    public long           DokumentRef { get; init; }
    public ZvFImportStats Stats       { get; init; } = new();
}