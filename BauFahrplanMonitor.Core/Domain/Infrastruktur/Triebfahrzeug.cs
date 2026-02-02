namespace BauFahrplanMonitor.Core.Domain.Infrastruktur;

public sealed class Triebfahrzeug
{
    public string Baureihe           { get; init; } = null!;
    public string Bezeichnung        { get; init; } = null!;
    public bool   Elektrifiziert     { get; init; }
    public bool   Triebwagen         { get; init; }
    public bool   AktiveNeigetechnik { get; init; }
    public int    KennungWert        { get; init; }
}