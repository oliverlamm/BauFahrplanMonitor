namespace BauFahrplanMonitor.Core.Dto;

public sealed record ZugTimelinePointDto(
    int       SeqNo,
    string    Rl100,
    string    Name,
    string    Type,
    string    Kbez,
    int       ArrivalMinute,
    int       DepartureMinute,
    TimeOnly? Arrival,
    TimeOnly? Departure,
    int       DayOffset
);