using System.Threading;
using System.Threading.Tasks;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Resolver;
using Microsoft.EntityFrameworkCore;

namespace BauFahrplanMonitor.Importer.Helper;

public sealed class ImportCoordinator(
    SharedReferenceResolver           resolver,
    IDbContextFactory<UjBauDbContext> dbFactory) {
    public async Task InitializeAsync(CancellationToken token)
    {
        await using var db = await dbFactory.CreateDbContextAsync(token);
        await resolver.WarmUpRegionCacheAsync(db, token);
    }
}