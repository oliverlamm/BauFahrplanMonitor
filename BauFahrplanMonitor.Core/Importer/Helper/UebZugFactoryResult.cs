using System.Collections.Generic;
using BauFahrplanMonitor.Importer.Dto.Ueb;

namespace BauFahrplanMonitor.Importer.Helper;

public sealed class UebZugFactoryResult {
    public required List<UebZugDto>                             Zuege      { get; init; }
    public required Dictionary<UebZugKey, List<UebRegelungDto>> Regelungen { get; init; }

    // Stats-Hilfen:
    public long SevsGelesen              { get; init; }
    public long SevsMitErsatzzug         { get; init; }
    public long ZuegeAusSevErzeugt       { get; init; }
    public long ErsatzzuegeAusSevErzeugt { get; init; }
}