namespace BauFahrplanMonitor.Core.Importer.Helper;

public sealed class ZvFImportStats {
    public int ZuegeInserted        { get; set; }
    public int ZuegeUpdated         { get; set; }
    public int AbweichungenInserted { get; set; }
    public int EntfallenInserted    { get; set; }

    public int ZuegeGesamt => ZuegeInserted + ZuegeUpdated;

    public string ToSummaryString() =>
        $"Züge={ZuegeGesamt}, Entfallen={EntfallenInserted}";

    public override string ToString() =>
        $"Züge +{ZuegeInserted}/~{ZuegeUpdated}, " +
        $"Abweichungen +{AbweichungenInserted}, "  +
        $"Entfallen +{EntfallenInserted}";
}