namespace BauFahrplanMonitor.Core.Importer.Dto.Fplo;

public sealed class FploHaltausfallDto {
    public long     ZugNr       { get; init; }
    public DateOnly Verkehrstag { get; init; }

    public string? AusfallenderHaltDs100 { get; init; }
    public string? AusfallenderHaltName  { get; init; }

    public string? ErsatzHaltDs100 { get; init; }
    public string? ErsatzHaltName  { get; init; }
}
