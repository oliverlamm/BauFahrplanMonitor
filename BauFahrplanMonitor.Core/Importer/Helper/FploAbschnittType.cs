namespace BauFahrplanMonitor.Importer.Helper;

public enum FploAbschnittType {
    Unbekannt = 0,

    Umleitung,
    ZusaetzlicheLeistung,
    Vorplanfahrt,
    VerspaetungRegelweg,

    // Sonderfälle
    Ausfall,
    SevAbschnitt
}