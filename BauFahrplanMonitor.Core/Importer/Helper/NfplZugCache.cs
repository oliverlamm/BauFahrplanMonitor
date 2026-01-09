using System.Collections.Concurrent;
using BauFahrplanMonitor.Core.Interfaces;
using BauFahrplanMonitor.Helpers;

namespace BauFahrplanMonitor.Core.Importer.Helper;

public sealed class NfplZugCache : INfplZugCache {

    private readonly ConcurrentDictionary<(long ZugNr, int Jahr), ZugCacheEntry>
        _cache = new();

    public Task<ZugCacheEntry> GetOrCreateAsync(
        long zugNr,
        int  fahrplanJahr) {

        return Task.FromResult(
            _cache.GetOrAdd(
                (zugNr, fahrplanJahr),
                _ => new ZugCacheEntry {
                    ZugId = 0 // 🔑 bewusst unpersistiert
                }));
    }
}