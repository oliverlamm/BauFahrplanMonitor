namespace BauFahrplanMonitor.Api.Dto;

public sealed class BasisLookupsDto {

    public IReadOnlyList<string> Zustaende { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<LookupItemDto> Typen { get; init; } =
        Array.Empty<LookupItemDto>();

    public IReadOnlyList<LookupItemDto> Regionen { get; init; } =
        Array.Empty<LookupItemDto>();

    public IReadOnlyList<LookupItemDto> Netzbezirke { get; init; } =
        Array.Empty<LookupItemDto>();
}

public sealed class LookupItemDto {
    public long   Id   { get; init; }
    public string Name { get; init; } = "";
}