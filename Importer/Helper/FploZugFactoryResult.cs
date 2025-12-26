using System.Collections.Generic;
using BauFahrplanMonitor.Importer.Dto.Fplo;
using BauFahrplanMonitor.Importer.Dto.Ueb;

namespace BauFahrplanMonitor.Importer.Helper;

public sealed class FploZugFactoryResult {
    public required List<FploZugDto>                             Zuege      { get; init; }
    public required Dictionary<FploZugKey, List<FploRegelungDto>> Regelungen { get; init; }

    // Stats-Hilfen:
    public long SevsGelesen              { get; init; }
    public long SevsMitErsatzzug         { get; init; }
    public long ZuegeAusSevErzeugt       { get; init; }
    public long ErsatzzuegeAusSevErzeugt { get; init; }
}