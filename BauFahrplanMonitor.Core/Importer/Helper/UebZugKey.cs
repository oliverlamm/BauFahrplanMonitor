namespace BauFahrplanMonitor.Core.Importer.Helper;

public readonly record struct UebZugKey(
    long     ZugNr,
    DateOnly Verkehrstag);