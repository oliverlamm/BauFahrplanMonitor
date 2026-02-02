namespace BauFahrplanMonitor.Core.Domain.Infrastruktur;

public sealed class InfrastrukturElement {
    public string         Id           { get; init; } = default!;
    public string         Bezeichnung  { get; init; } = default!;
    public int            FahrplanJahr { get; set; }
    public DateTimeOffset GueltigVon   { get; set; }
    public DateTimeOffset GueltigBis   { get; set; }

    // optional, später:
    // public string? Typ { get; init; }
    // public bool Elektrifiziert { get; init; }
}