using System;
using System.Threading.Tasks;
using BauFahrplanMonitor.Helpers;

namespace BauFahrplanMonitor.Interfaces;

public interface INfplZugCache {
    Task<ZugCacheEntry> GetOrCreateAsync(
        long zugNr,
        int  fahrplanJahr);
}
