namespace BauFahrplanMonitor.Trassenfinder.Configuration;

public sealed class TrassenfinderOptions {
    public string BaseUrl  { get; init; } = default!;
    public string ApiToken { get; init; } = default!;
}