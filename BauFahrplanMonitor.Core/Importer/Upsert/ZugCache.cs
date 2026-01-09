using System.Collections.Concurrent;

namespace BauFahrplanMonitor.Core.Importer.Upsert;

public sealed class ZvFZugCache {
    // Key: "dokRef|zugnr|yyyy-MM-dd"
    private readonly ConcurrentDictionary<string, long> _cache = new();

    public bool TryGet(long dokumentRef, long zugnr, DateOnly tag, out long zugRef)
        => _cache.TryGetValue(Key(dokumentRef, zugnr, tag), out zugRef);

    public void Set(long dokumentRef, long zugnr, DateOnly tag, long zugRef)
        => _cache[Key(dokumentRef, zugnr, tag)] = zugRef;

    private static string Key(long dokRef, long zugnr, DateOnly tag)
        => $"{dokRef}|{zugnr}|{tag:yyyy-MM-dd}";

    // optional
    public void Clear() => _cache.Clear();
}