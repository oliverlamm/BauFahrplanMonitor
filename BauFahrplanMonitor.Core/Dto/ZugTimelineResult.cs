namespace BauFahrplanMonitor.Core.Dto;

public sealed record ZugTimelineResult(
    int                             ZugNr,
    DateOnly                        Date,
    IReadOnlyList<ZugTimelinePointDto> Timeline
);
