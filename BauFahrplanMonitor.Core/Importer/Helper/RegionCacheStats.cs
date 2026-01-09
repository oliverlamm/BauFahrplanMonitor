namespace BauFahrplanMonitor.Core.Importer.Helper;

public sealed class RegionCacheStats {
    public long CacheHits;
    public long CacheMisses;
    public long DbHits;

    public override string ToString()
        => $"CacheHits={CacheHits}, DbHits={DbHits}, Misses={CacheMisses}";
}