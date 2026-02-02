using BauFahrplanMonitor.Core.Interfaces;
using BauFahrplanMonitor.Core.Jobs;
using BauFahrplanMonitor.Trassenfinder.Generated;

namespace BauFahrplanMonitor.Trassenfinder.Services;

public sealed class TrassenfinderRefreshJob(
    TrassenfinderClient client
) : ITrassenfinderRefreshJob {
    private readonly TrassenfinderClient _client = client;

    public async Task RefreshInfrastrukturAsync(
        long                                id,
        IProgress<TrassenfinderInfraStatus> progress,
        CancellationToken                   token = default) {
        progress.Report(new TrassenfinderInfraStatus {
            Percent = 0,
            Message = "Lade Infrastruktur…"
        });

        var infraDto =
            await _client.Get_infrastrukturAsync(id, token);

        if (infraDto is null)
            throw new InvalidOperationException(
                $"Infrastruktur {id} nicht gefunden");

        // 🔒 NULL-SICHER!
        var betriebsstellen =
            infraDto.Ordnungsrahmen?.Betriebsstellen
            ?? Array.Empty<Betriebsstelle>();

        var fahrzeuge =
            infraDto.Stammdaten?.Triebfahrzeuge
            ?? Array.Empty<Triebfahrzeug>();

        var total = betriebsstellen.Count + fahrzeuge.Count;

        if (total == 0) {
            progress.Report(new TrassenfinderInfraStatus {
                Percent = 100,
                Message = "Keine Daten vorhanden"
            });
            return;
        }

        var done = 0;

        foreach (var bs in betriebsstellen) {
            token.ThrowIfCancellationRequested();

            // TODO: DB-Update Betriebsstelle
            done++;

            progress.Report(new TrassenfinderInfraStatus {
                Percent = done * 100 / total,
                Message = $"Betriebsstelle {bs.Ds100}"
            });
        }

        foreach (var fz in fahrzeuge) {
            token.ThrowIfCancellationRequested();

            // TODO: DB-Update Fahrzeug
            done++;

            progress.Report(new TrassenfinderInfraStatus {
                Percent = done * 100 / total,
                Message = $"Fahrzeug {fz.Bezeichnung}"
            });
        }
    }
}