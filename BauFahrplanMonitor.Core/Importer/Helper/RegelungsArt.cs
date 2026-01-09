namespace BauFahrplanMonitor.Core.Importer.Helper;

public static class RegelungsArt {
    // ===== Basis / Abschnitt (immer abgeleitet, KEIN Json) =====
    public const string Umleitung            = "Umleitung";
    public const string ZusaetzlicheLeistung = "Zusätzlicher Leistung";
    public const string Vorplanfahrt         = "Vorplanfahrt";
    public const string VerspaetungRegelweg  = "Verspätung";
    public const string SevAbschnitt         = "Ersatzzug";

    // ===== Originäre XML-Elemente (MIT Json) =====
    public const string Ausfall         = "Ausfall";
    public const string Sev             = "Teilausfall";
    public const string Haltausfall     = "Haltausfall";
    public const string Zurueckgehalten = "Zurückgehalten";
    public const string Zugparameter    = "Zugparameter";
}