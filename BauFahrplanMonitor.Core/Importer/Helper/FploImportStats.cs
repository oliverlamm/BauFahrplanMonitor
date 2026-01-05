namespace BauFahrplanMonitor.Importer.Helper;

public sealed class FploImportStats {
    public long Dokumente { get; set; }

    public long ZuegeInserted { get; set; }
    public long ZuegeUpdated  { get; set; }

    // fachliche Zähler
    public long SevsGelesen     { get; set; }
    public long Haltausfall     { get; set; }
    public long Zurueckgehalten { get; set; }
    public long Zugparameter    { get; set; }

    public long ZuegeGesamt => ZuegeInserted + ZuegeUpdated;

    // 👇 DAS ist die Abschluss-Zusammenfassung
    public string ToSummaryString() =>
        $"Züge={ZuegeGesamt}, SEV={SevsGelesen}, "                       +
        $"Haltausfall={Haltausfall}, Zurückgehalten={Zurueckgehalten}, " +
        $"Zugparameter={Zugparameter}";

    // 👇 Detail / Debug (darf gern mehr enthalten)
    public override string ToString() =>
        $"Dok={Dokumente}, Z+={ZuegeInserted}, Z~={ZuegeUpdated}, " +
        $"SEV={SevsGelesen}, Haltausfall={Haltausfall}, "           +
        $"Zurückgehalten={Zurueckgehalten}, Zugparameter={Zugparameter}";
}