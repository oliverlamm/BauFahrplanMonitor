namespace BauFahrplanMonitor.Api.Dto;

public sealed class Bst2StreckeDto {
    public long    Id       { get; init; }
    public long    VzG      { get; init; }
    public long    KmI      { get; init; }
    public string? KmL      { get; init; }
    public bool    IstBasis { get; init; }
}