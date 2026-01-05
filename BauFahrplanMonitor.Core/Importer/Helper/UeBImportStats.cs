namespace BauFahrplanMonitor.Importer.Helper;

public sealed class UeBImportStats {
    public long Dokumente { get; set; }

    public long ZuegeInserted { get; set; }
    public long ZuegeUpdated  { get; set; }

    public long SevsGelesen { get; set; }

    public long ZuegeGesamt => ZuegeInserted + ZuegeUpdated;

    public string ToSummaryString() =>
        $"Züge={ZuegeGesamt}, SEV={SevsGelesen}";

    public override string ToString() =>
        $"Dok={Dokumente}, Z+={ZuegeInserted}, Z~={ZuegeUpdated}, " +
        $"SEV={SevsGelesen}, ...";
}