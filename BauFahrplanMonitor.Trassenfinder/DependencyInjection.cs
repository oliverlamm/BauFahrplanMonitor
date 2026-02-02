using System.Net.Http.Headers;
using BauFahrplanMonitor.Core.Interfaces;
using BauFahrplanMonitor.Trassenfinder.Configuration;
using BauFahrplanMonitor.Trassenfinder.Generated;
using BauFahrplanMonitor.Trassenfinder.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BauFahrplanMonitor.Trassenfinder;

public static class DependencyInjection {
    public static IServiceCollection AddTrassenfinder(
        this IServiceCollection services,
        IConfiguration          configuration) {
        // -------------------------------
        // Options
        // -------------------------------
        services.Configure<TrassenfinderOptions>(
            configuration.GetSection("Trassenfinder"));

        // -------------------------------
        // HTTP Client (Generated)
        // -------------------------------
        services.AddHttpClient<TrassenfinderClient>((sp, client) => {
            var options = sp.GetRequiredService<IOptions<TrassenfinderOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", options.ApiToken);
        });

        // -------------------------------
        // Services
        // -------------------------------
        services.AddScoped<ITrassenfinderService, TrassenfinderService>();

        // ✅ HIER: Refresh-Job registrieren
        services.AddScoped<ITrassenfinderRefreshJob, TrassenfinderRefreshJob>();

        return services;
    }
}