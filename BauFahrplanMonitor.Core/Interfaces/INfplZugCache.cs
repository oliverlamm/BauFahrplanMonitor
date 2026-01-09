using BauFahrplanMonitor.Helpers;

namespace BauFahrplanMonitor.Core.Interfaces;

public interface INfplZugCache {
    Task<ZugCacheEntry> GetOrCreateAsync(
        long zugNr,
        int  fahrplanJahr);
}
