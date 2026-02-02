using BauFahrplanMonitor.Api.Jobs;

namespace BauFahrplanMonitor.Api.Dto;

public class TrassenfinderJobStatusDto {
    public string   JobId    { get; init; } = default!;
    public TrassenfinderJobState State    { get; init; }
    public int      Progress { get; init; }
    public string?  Message  { get; init; }
}