namespace BauFahrplanMonitor.Importer.Helper;

public sealed class FploImportStats {
    public long Dokumente { get; set; }

    public long ZuegeInserted { get; set; }
    public long ZuegeUpdated  { get; set; }

    public long KnotenzeitenInserted { get; set; }

    public long RegelungenInserted { get; set; }

    // --- Fplo-spezifisch / SEV ---
    public long SevsGelesen      { get; set; } // Anzahl <sev> Einträge im Dokument
    public long SevsMitErsatzzug { get; set; } // wie viele <sev> enthalten <ersatzzug>

    public long ZuegeAusSevErzeugt { get; set; } // Primärzüge, die nur aus SEV entstanden

    public long
        ErsatzzuegeAusSevErzeugt { get; set; } // Ersatzzüge, die aus SEV entstanden (nicht schon im <zug>-Block)

    public long SezKundeRefFallbackZero   { get; set; } // SEV-Zug ohne Betreiber -> KundeRef=0
    public int  SevKundeRefFallbackHeader { get; set; }
    public int  SevKundeRefFallbackZero   { get; set; }

    public override string ToString()
        => $"Dok={Dokumente}, Z+={ZuegeInserted}, Z~={ZuegeUpdated}, "                     +
           $"Knoten+={KnotenzeitenInserted}, Reg+={RegelungenInserted}, "                  +
           $"SEV={SevsGelesen} (mitErsatzzug={SevsMitErsatzzug}), "                        +
           $"ZugAusSEV={ZuegeAusSevErzeugt}, ErsatzzugAusSEV={ErsatzzuegeAusSevErzeugt}, " +
           $"KundeRef(SEV)={SezKundeRefFallbackZero}, SEVFallbackHeader={SevKundeRefFallbackHeader}, SEVFallback={SevKundeRefFallbackZero}";
}