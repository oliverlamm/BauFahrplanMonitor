namespace BauFahrplanMonitor.Core.Importer.Helper;

public static class ImportSteps {
    public const int TotalSteps = 6;

    public const int Read     = 1;
    public const int Map      = 2;
    public const int Merge    = 3;
    public const int Upsert   = 4;
    public const int Cleanup  = 5;
    public const int Finalize = 6;
    
    // 🔑 Zentrale Texte
    public static string GetText(int step) => step switch {
        Read     => "Datei lesen",
        Map      => "Daten abbilden",
        Merge    => "Daten zusammenführen",
        Upsert   => "Daten speichern",
        Cleanup  => "Aufräumen",
        Finalize => "Abschluss",
        _        => "Unbekannter Schritt"
    };
}