namespace BauFahrplanMonitor.Core.Importer.Dto.Nfpl;

public sealed class NetzfahrplanVerlaufDto {
    public long Seq { get; init; }

    // aus entry.posID
    public string BstRl100 { get; init; } = null!;

    public string Type { get; init; } = null!;

    public TimeOnly? PublishedArrival   { get; init; }
    public TimeOnly  PublishedDeparture { get; init; }

    public string? Remarks { get; init; }

    // 🔑 vererbte Service-Daten
    public string?   ServiceBitmask     { get; init; }
    public DateOnly? ServiceStartdate   { get; init; }
    public DateOnly? ServiceEnddate     { get; init; }
    public string?   ServiceDescription { get; init; }
}
