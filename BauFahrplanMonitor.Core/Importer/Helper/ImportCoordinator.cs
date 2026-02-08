using BauFahrplanMonitor.Core.Data;
using BauFahrplanMonitor.Core.Resolver;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Core.Importer.Helper;

public sealed class ImportCoordinator(
    SharedReferenceResolver           resolver,
    IDbContextFactory<UjBauDbContext> dbFactory) {
    public async Task InitializeAsync(CancellationToken token) {
        await using var db = await dbFactory.CreateDbContextAsync(token);
        await resolver.WarmUpRegionCacheAsync(db, token);
    }
}