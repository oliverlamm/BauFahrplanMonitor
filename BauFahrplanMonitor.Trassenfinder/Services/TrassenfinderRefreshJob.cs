using BauFahrplanMonitor.Core.Data;
using BauFahrplanMonitor.Core.Interfaces;
using BauFahrplanMonitor.Core.Jobs;
using BauFahrplanMonitor.Core.Models;
using BauFahrplanMonitor.Core.Resolver;
using BauFahrplanMonitor.Trassenfinder.Generated;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Betriebsstelle = BauFahrplanMonitor.Trassenfinder.Generated.Betriebsstelle;
using Triebfahrzeug = BauFahrplanMonitor.Trassenfinder.Generated.Triebfahrzeug;

namespace BauFahrplanMonitor.Trassenfinder.Services;

public sealed class TrassenfinderRefreshJob(
    TrassenfinderClient               client,
    IDbContextFactory<UjBauDbContext> dbFactory,
    SharedReferenceResolver           resolver,
    ILogger<TrassenfinderRefreshJob>  logger
) : ITrassenfinderRefreshJob {

    private readonly TrassenfinderClient               _client    = client;
    private readonly IDbContextFactory<UjBauDbContext> _dbFactory = dbFactory;
    private readonly SharedReferenceResolver           _resolver  = resolver;
    private readonly ILogger<TrassenfinderRefreshJob>  _logger    = logger;

    private static Point CreatePointWgs84(double lat, double lon)
        => new(lon, lat) {
            SRID = 4326
        };

    // ------------------------------------------------------------
    // Mutterbetriebsstellen -> BasisBetriebsstellenbereich
    // Annahme: Tabelle speichert Parent/Child als Refs
    // BstRef = Mutter, BstChildRef = Kind
    // ------------------------------------------------------------
    private async Task<int> UpdateMutterbetriebsstellenAsync(
        IEnumerable<Mutter_betriebsstelle>  items,
        UjBauDbContext                      db,
        IProgress<TrassenfinderInfraStatus> progress,
        int                                 done,
        int                                 total,
        CancellationToken                   token) {

        foreach (var mb in items) {
            token.ThrowIfCancellationRequested();

            var parentRl100 = mb.Ds100?.Trim();
            if (string.IsNullOrWhiteSpace(parentRl100)) {
                done++;
                continue;
            }

            var parentRef = await _resolver.ResolveOrCreateBetriebsstelleAsync(db, parentRl100, token);
            if (parentRef <= 0) {
                done++;
                continue;
            }

            // (Optional) Name der Mutter in BasisBetriebsstelle aktualisieren (falls gewünscht)
            // -> Das passiert sowieso schon im Betriebsstellen-Loop; hier nur falls du es extra brauchst.

            // Cleanup: alle bisherigen Kinder-Zuordnungen für diesen Parent löschen
            // (Wenn du stattdessen eine JSON-Spalte/SetTochterRl100 hast, dann diesen Block ersetzen!)
            var existing = await db.BasisBetriebsstellenbereich
                .Where(x => x.BstRef == parentRef)
                .ToListAsync(token);

            if (existing.Count > 0) {
                db.BasisBetriebsstellenbereich.RemoveRange(existing);
            }

            // Neue Kinder anlegen
            var childs = mb.Tochterbetriebsstellen ?? new List<string>();

            foreach (var childRl100Raw in childs) {
                var childRl100 = childRl100Raw?.Trim();
                if (string.IsNullOrWhiteSpace(childRl100))
                    continue;

                var childRef = await _resolver.ResolveOrCreateBetriebsstelleAsync(db, childRl100, token);
                if (childRef <= 0)
                    continue;

                db.BasisBetriebsstellenbereich.Add(new BasisBetriebsstellenbereich {
                    BstRef      = parentRef,
                    BstChildRef = childRef
                });
            }

            done++;

            progress.Report(new TrassenfinderInfraStatus {
                Percent = (int)Math.Round(done * 100.0 / total),
                Message = $"Bereich {parentRl100}"
            });

            if (done % 200 == 0) // Mutterbereiche sind meist weniger – ruhig gröber speichern
                await db.SaveChangesAsync(token);
        }
        return done;
    }

    // ------------------------------------------------------------
    // Streckensegmente -> BasisStreckeAbschnitt (Refs!)
    // VzGRef -> BasisStrecke.Id
    // VonBstRef / BisBstRef -> BasisBetriebsstelle.Id
    // ------------------------------------------------------------
    private async Task<int> UpdateStreckensegmenteAsync(
        IEnumerable<Streckensegment>        segmente,
        UjBauDbContext                      db,
        IProgress<TrassenfinderInfraStatus> progress,
        int                                 done,
        int                                 total,
        CancellationToken                   token) {

        foreach (var s in segmente) {
            token.ThrowIfCancellationRequested();

            // StreckeRef ist bei dir read-only -> wenn nicht vorhanden: skip
            var vzgRef = await _resolver.ResolveStreckeAsync(db, s.Streckennummer, token);
            if (vzgRef <= 0) {
                done++;
                continue;
            }

            var vonRef = await _resolver.ResolveOrCreateBetriebsstelleAsync(db, s.Von, token);
            var bisRef = await _resolver.ResolveOrCreateBetriebsstelleAsync(db, s.Bis, token);

            if (vonRef <= 0 || bisRef <= 0) {
                done++;
                continue;
            }

            // ⚠️ Propertynamen ggf. anpassen (VzGRef/VonBstRef/BisBstRef)
            var entity = await db.BasisStreckeAbschnitt
                .FirstOrDefaultAsync(a =>
                        a.StreckeRef == vzgRef &&
                        a.VonBstRef  == vonRef &&
                        a.BisBstRef  == bisRef,
                    token);

            if (entity == null) {
                entity = new BasisStreckeAbschnitt {
                    StreckeRef = vzgRef,
                    VonBstRef  = vonRef,
                    BisBstRef  = bisRef,
                    VonKmI     = (decimal?)s.Von_km,
                    BisKmI     = (decimal?)s.Bis_km
                };
                db.BasisStreckeAbschnitt.Add(entity);
            }
            else {
                entity.VonKmI = (decimal?)s.Von_km;
                entity.BisKmI = (decimal?)s.Bis_km;
            }

            done++;

            progress.Report(new TrassenfinderInfraStatus {
                Percent = (int)Math.Round(done * 100.0 / total),
                Message = $"Strecke {s.Streckennummer}"
            });

            if (done % 500 == 0) // Segmente können 30k sein
                await db.SaveChangesAsync(token);
        }
        return done;
    }

    private async Task<int> UpdateTriebfahrzeugeAsync(
        IEnumerable<Triebfahrzeug>          fahrzeuge,
        UjBauDbContext                      db,
        IProgress<TrassenfinderInfraStatus> progress,
        int                                 done,
        int                                 total,
        CancellationToken                   token) {

        foreach (var fz in fahrzeuge) {
            token.ThrowIfCancellationRequested();

            // 🔑 Business-Key
            var hn = fz.Hauptnummer;
            var un = fz.Unternummer;

            if (hn.Length < 1 || un < 0) {
                done++;
                continue;
            }

            var entity = await db.BasisTriebfahrzeuge
                .FirstOrDefaultAsync(x =>
                        x.Hauptnummer == hn &&
                        x.Unternummer == un,
                    token);

            if (entity == null) {
                entity = new BasisTriebfahrzeuge {
                    Hauptnummer = hn,
                    Unternummer = un,
                };
                db.BasisTriebfahrzeuge.Add(entity);
            }

            // ✍️ Aktualisieren (immer überschreiben – Trassenfinder ist führend)
            entity.Baureihenname      = fz.Baureihenname;
            entity.Bezeichnung        = fz.Bezeichnung;
            entity.Elektrifiziert     = fz.Elektrifiziert;
            entity.Triebwagen         = fz.Triebwagen;
            entity.AktiveNeigetechnik = fz.Aktive_neigetechnik;
            entity.KennungWert        = fz.Kennung_wert;

            done++;

            progress.Report(new TrassenfinderInfraStatus {
                Percent = (int)Math.Round(done * 100.0 / total),
                Message = $"Fahrzeug {entity.Baureihenname}"
            });

            // 🚦 Batch-Commit (Fahrzeuge sind oft viele)
            if (done % 500 == 0)
                await db.SaveChangesAsync(token);
        }
        return done;
    }

    // ------------------------------------------------------------
    // Hauptmethode
    // ------------------------------------------------------------
    public async Task RefreshInfrastrukturAsync(
        long                                id,
        IProgress<TrassenfinderInfraStatus> progress,
        CancellationToken                   token = default) {

        _logger.LogInformation("Trassenfinder-Refresh gestartet: InfrastrukturId={InfraId}", id);

        progress.Report(new TrassenfinderInfraStatus {
            Percent = 0,
            Message = "Lade Infrastruktur…"
        });

        var infraDto = await _client.Get_infrastrukturAsync(id, token);
        if (infraDto is null)
            throw new InvalidOperationException($"Infrastruktur {id} nicht gefunden");

        var ordnung = infraDto.Ordnungsrahmen;

        var betriebsstellen = ordnung?.Betriebsstellen            ?? Array.Empty<Betriebsstelle>();
        var mutterbereiche  = ordnung?.Mutter_betriebsstellen     ?? Array.Empty<Mutter_betriebsstelle>();
        var segmente        = ordnung?.Streckensegmente           ?? Array.Empty<Streckensegment>();
        var fahrzeuge       = infraDto.Stammdaten?.Triebfahrzeuge ?? Array.Empty<Triebfahrzeug>();

        // ✅ Total muss ALLES enthalten, was du verarbeitest
        var total =
            betriebsstellen.Count +
            mutterbereiche.Count  +
            segmente.Count        +
            fahrzeuge.Count;

        _logger.LogInformation(
            "Infrastruktur {InfraId}: BS={Bs} Mutter={Mb} Segmente={Seg} Fahrzeuge={Fz} Total={Total}",
            id, betriebsstellen.Count, mutterbereiche.Count, segmente.Count, fahrzeuge.Count, total);

        if (total == 0) {
            progress.Report(new TrassenfinderInfraStatus {
                Percent = 100,
                Message = "Keine Daten vorhanden"
            });
            return;
        }

        var             done = 0;
        await using var db   = await _dbFactory.CreateDbContextAsync(token);

        // ------------------------------------------------------------
        // 1) Betriebsstellen -> Name + Plc, Geo nur wenn Shape leer
        // ------------------------------------------------------------
        foreach (var bs in betriebsstellen) {
            token.ThrowIfCancellationRequested();

            var rl100 = bs.Ds100?.Trim();
            if (string.IsNullOrWhiteSpace(rl100)) {
                done++;
                continue;
            }

            var bstRef = await _resolver.ResolveOrCreateBetriebsstelleAsync(db, rl100, token);
            if (bstRef <= 0) {
                done++;
                continue;
            }

            // BasisBetriebsstelle aktualisieren (Name/Plc)
            var entity = await db.BasisBetriebsstelle
                .FirstOrDefaultAsync(b => b.Id == bstRef, token);

            if (entity != null) {
                entity.Name = bs.Langname?.Trim();

                if (!string.IsNullOrWhiteSpace(bs.Primary_location_code))
                    entity.Plc = bs.Primary_location_code.Trim();
            }

            // Geo: in Schnittstelle kommt geo_koordinaten { breite, laenge } WGS84
            if (entity != null && bs.Geo_koordinaten is not null) {
                var lat = bs.Geo_koordinaten.Breite;
                var lon = bs.Geo_koordinaten.Laenge;

                if (lat != 0 && lon != 0) {
                    // Nur dort setzen, wo Shape leer ist.
                    // Wichtig: Diese Tabelle ist bst<->strecke, daher suchen wir einen Eintrag mit BstRef und Shape==null.
                    // Wenn du genauer sein willst: zusätzlich StreckeRef/ kmL matchen (falls vorhanden).
                    var geoRow = await db.BasisBetriebsstelle2strecke
                        .FirstOrDefaultAsync(x => x.BstRef == bstRef && x.Shape == null, token);

                    if (geoRow != null) {
                        geoRow.Shape = CreatePointWgs84(lat, lon);
                    }
                }
            }

            done++;

            if (done % 200 == 0) // bei 8k+ ruhig blockweise
                await db.SaveChangesAsync(token);

            progress.Report(new TrassenfinderInfraStatus {
                Percent = (int)Math.Round(done * 100.0 / total),
                Message = $"Betriebsstelle {rl100}"
            });
        }

        await db.SaveChangesAsync(token);

        // ------------------------------------------------------------
        // 2) Mutterbetriebsstellen
        // ------------------------------------------------------------
        done = await UpdateMutterbetriebsstellenAsync(
            mutterbereiche, db, progress, done, total, token);

        await db.SaveChangesAsync(token);

        // ------------------------------------------------------------
        // 3) Streckensegmente
        // ------------------------------------------------------------
        done = await UpdateStreckensegmenteAsync(
            segmente, db, progress, done, total, token);

        await db.SaveChangesAsync(token);

        // ------------------------------------------------------------
        // 4) Fahrzeuge (noch ohne DB-Write – aber Fortschritt zählt mit)
        // ------------------------------------------------------------
        done = await UpdateTriebfahrzeugeAsync(
            fahrzeuge,
            db,
            progress,
            done,
            total,
            token
        );

        await db.SaveChangesAsync(token);

        _logger.LogInformation("Trassenfinder-Refresh abgeschlossen: InfrastrukturId={InfraId}", id);

        progress.Report(new TrassenfinderInfraStatus {
            Percent = 100,
            Message = "Fertig"
        });
    }
}