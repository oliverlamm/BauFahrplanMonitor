using BauFahrplanMonitor.Core.Importer.Dto.Fplo;

namespace BauFahrplanMonitor.Core.Importer.Helper;

public sealed class FploZugFactoryResult {
    public required List<FploZugDto>                              Zuege      { get; init; }
    public required Dictionary<FploZugKey, List<FploRegelungDto>> Regelungen { get; init; }

    // 🔹 Transformation / Herkunft
    public long SevsGelesen              { get; init; }
    public long SevsMitErsatzzug         { get; init; }
    public long ZuegeAusSevErzeugt       { get; init; }
    public long ErsatzzuegeAusSevErzeugt { get; init; }

    // 🔹 Fachliche Regelungen (DEDUPLIZIERT!)
    public long SevRegelungen             { get; init; }
    public long HaltausfallRegelungen     { get; init; }
    public long ZurueckgehaltenRegelungen { get; init; }
    public long ZugparameterRegelungen    { get; init; }
}
