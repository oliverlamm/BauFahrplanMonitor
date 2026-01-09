namespace BauFahrplanMonitor.Core.Importer.Helper;

public readonly record struct FploZugKey(
    long     ZugNr,
    DateOnly Verkehrstag);