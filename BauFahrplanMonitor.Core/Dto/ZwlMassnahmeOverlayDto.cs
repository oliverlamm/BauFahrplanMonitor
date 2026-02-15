namespace BauFahrplanMonitor.Core.Dto;

public sealed class ZwlMassnahmeOverlayDto {
    public string VonRl100 { get; init; } = default!;
    public string BisRl100 { get; init; } = default!;

    public DateTime MassnahmeBeginn { get; init; }
    public DateTime MassnahmeEnde   { get; init; }

    public string Regelungen  { get; init; } = default!;
    public string VzgListe    { get; init; } = default!;
    public bool   Durchgehend { get; init; }
    public string Zeitraum    { get; init; } = default;
}