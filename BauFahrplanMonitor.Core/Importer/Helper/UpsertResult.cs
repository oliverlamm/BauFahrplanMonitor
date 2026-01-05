namespace BauFahrplanMonitor.Importer.Helper;

public sealed class UpsertResult {
    public long           DokumentRef { get; init; }
    public ZvFImportStats ZvFStats    { get; init; } = new();
    public UeBImportStats UeBStats    { get; init; } = new();
    public FploImportStats FploStats    { get; init; } = new();
}