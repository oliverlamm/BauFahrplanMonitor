using System.Collections.Concurrent;
using System.Threading;

namespace BauFahrplanMonitor.Helpers;

public sealed class ZugCacheEntry {
    /// <summary>
    /// Datenbank-ID von nfpl_zug
    /// </summary>
    public long ZugId { get; set; }

    /// <summary>
    /// Synchronisations-Lock für alle Operationen an diesem Zug
    /// </summary>
    public SemaphoreSlim Lock { get; } = new(1, 1);

    /// <summary>
    /// Cache für Varianten-IDs:
    /// Key = (TrainId, TrainNumber)
    /// Value = nfpl_zug_variante.id
    /// </summary>
    public ConcurrentDictionary<(long? TrainId, string? TrainNumber), long>
        Varianten { get; } = new();
}