using BauFahrplanMonitor.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace BauFahrplanMonitor.Core.Data;

public sealed class UjBauDbContextFactory
    : IDesignTimeDbContextFactory<UjBauDbContext> {
    public UjBauDbContext CreateDbContext(string[] args) {
        // --------------------------------------------
        // Services aufbauen (minimal)
        // --------------------------------------------
        var services = new ServiceCollection();

        services.AddSingleton<ConfigService>();

        var sp     = services.BuildServiceProvider();
        var config = sp.GetRequiredService<ConfigService>();

        // --------------------------------------------
        // DbContext Options
        // --------------------------------------------
        var options = new DbContextOptionsBuilder<UjBauDbContext>()
            .UseNpgsql(
                config.BuildConnectionString(),
                o => {
                    o.UseNetTopologySuite();
                    o.MaxBatchSize(100);
                })
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
            .Options;

        return new UjBauDbContext(options);
    }
}