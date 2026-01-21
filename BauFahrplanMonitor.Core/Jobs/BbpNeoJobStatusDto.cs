using BauFahrplanMonitor.Core.Jobs;

namespace BauFahrplanMonitor.Api.Dto;

public sealed class BbpNeoJobStatusDto {
    public ImportJobState State { get; init; }

    public DateTime? StartedAt  { get; init; }
    public DateTime? FinishedAt { get; init; }

    public string? CurrentFile { get; init; }

    public int MassnahmenGesamt { get; init; }
    public int MassnahmenFertig { get; init; }

    public int Regelungen { get; init; }
    public int BvE        { get; init; }
    public int Aps        { get; init; }
    public int Iav        { get; init; }

    public int Errors { get; init; }

}