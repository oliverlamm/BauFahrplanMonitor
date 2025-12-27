using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Importer.Dto.BbpNeo;
using BauFahrplanMonitor.Importer.Interface;
using BauFahrplanMonitor.Models;
using BauFahrplanMonitor.Resolver;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BauFahrplanMonitor.Importer.Upsert;

public sealed class BbpNeoUpserter : IBbpNeoUpsertService {
    private readonly ILogger<BbpNeoUpserter> _logger;
    private readonly SharedReferenceResolver _resolver;

    public BbpNeoUpserter(
        ILogger<BbpNeoUpserter> logger,
        SharedReferenceResolver resolver) {
        _logger   = logger;
        _resolver = resolver;
    }

    // =====================================================================
    // ENTRY
    // =====================================================================
    public async Task UpsertMassnahmeWithChildrenAsync(
        UjBauDbContext        db,
        BbpNeoMassnahme       massnahme,
        IReadOnlyList<string> warnings,
        CancellationToken     token) {
        token.ThrowIfCancellationRequested();

        using var scope = _logger.BeginScope(new Dictionary<string, object> {
            ["Importer"] = "BBPNeo",
            ["MasId"]    = massnahme.MasId
        });

        // -------------------------------------------------
        // Warnungen aus dem Normalizer
        // -------------------------------------------------
        foreach (var w in warnings) {
            _logger.LogWarning("BBPNeo Normalizer-Warnung: {Warning}", w);
        }

        try {
            _logger.LogDebug("Upsert Maßnahme gestartet");
            var masEntity = await UpsertMassnahmeAsync(db, massnahme, token);

            foreach (var regelung in massnahme.Regelungen) {
                await UpsertRegelungAsync(db, masEntity.Id, regelung, token);
            }

            _logger.LogDebug("Upsert Maßnahme abgeschlossen");
        }
        catch (OperationCanceledException) {
            _logger.LogInformation("Upsert Maßnahme abgebrochen");
            throw;
        }
        catch (Exception ex) {
            _logger.LogError(
                ex,
                "Fehler beim Upsert der Maßnahme");
            throw;
        }
    }

    // =====================================================================
    // MASSNAHME
    // =====================================================================
    private async Task<BbpneoMassnahme> UpsertMassnahmeAsync(
        UjBauDbContext    db,
        BbpNeoMassnahme   mas,
        CancellationToken token) {
        var entity = await db.BbpneoMassnahme
            .FirstOrDefaultAsync(x => x.MasId == mas.MasId, token);

        if (entity == null) {
            entity = new BbpneoMassnahme {
                MasId = mas.MasId
            };
            db.Add(entity);
        }

        var bstVon = await _resolver.ResolveOrCreateBetriebsstelleAsync(db, mas.MasVonBstDs100, token);
        var bstBis = await _resolver.ResolveOrCreateBetriebsstelleAsync(db, mas.MasBisBstDs100, token);
        var strVon = await _resolver.ResolveStreckeAsync(db, mas.MasVonVzG);
        var strBis = await _resolver.ResolveStreckeAsync(db, mas.MasBisVzG);
        
        entity.Aktiv            = mas.Aktiv;
        entity.MasBeginn        = mas.Beginn    ?? DateTime.MinValue;
        entity.MasEnde          = mas.Ende      ?? DateTime.MinValue;
        entity.RegionRef        = mas.RegionRef ?? 0;
        entity.Arbeiten         = mas.Arbeiten  ?? string.Empty;
        entity.ArtDerArbeit     = mas.ArtDerArbeit;
        entity.Genehmigung      = mas.Genehmigung;
        entity.AnforderungBbzr  = mas.AnforderungBbzr;
        entity.MasVonBst2strRef = await _resolver.ResolveBst2StrAsync(db, bstVon, strVon, token: token);
        entity.MasBisBst2strRef = await _resolver.ResolveBst2StrAsync(db, bstBis, strBis, token: token);
        entity.MasVonKmL        = mas.MasVonKmL;
        entity.MasBisKmL        = mas.MasBisKmL;

        await db.SaveChangesAsync(token);
        return entity;
    }

    // =====================================================================
    // REGELUNG
    // =====================================================================
    private async Task UpsertRegelungAsync(
        UjBauDbContext    db,
        long              masRef,
        BbpNeoRegelung    reg,
        CancellationToken token) {
        var entity = await db.BbpneoMassnahmeRegelung
            .FirstOrDefaultAsync(x => x.RegId == reg.RegId, token);

        if (entity == null) {
            entity = new BbpneoMassnahmeRegelung {
                RegId        = reg.RegId,
                BbpneoMasRef = masRef
            };
            db.Add(entity);
        }

        var bstVon = await _resolver.ResolveOrCreateBetriebsstelleAsync(db, reg.VonBstDs100, token);
        var bstBis = await _resolver.ResolveOrCreateBetriebsstelleAsync(db, reg.BisBstDs100, token);
        var strVon = await _resolver.ResolveStreckeAsync(db, reg.VonVzG);
        var strBis = await _resolver.ResolveStreckeAsync(db, reg.BisVzG);

        entity.Aktiv         = reg.Aktiv;
        entity.Beginn        = reg.Beginn   ?? DateTime.MinValue;
        entity.Ende          = reg.Ende     ?? DateTime.MinValue;
        entity.Bplart        = reg.BplArt   ?? string.Empty;
        entity.Zeitraum      = reg.Zeitraum ?? string.Empty;
        entity.Richtung      = reg.Richtung ?? 0;
        entity.RegelungKurz  = reg.RegelungKurz;
        entity.RegelungLang  = reg.RegelungLang;
        entity.Durchgehend   = reg.Durchgehend  ?? false;
        entity.Schichtweise  = reg.Schichtweise ?? false;
        entity.Bst2strVonRef = await _resolver.ResolveBst2StrAsync(db, bstVon, strVon, token: token);
        entity.Bst2strBisRef = await _resolver.ResolveBst2StrAsync(db, bstBis, strBis, token: token);

        await db.SaveChangesAsync(token);

        foreach (var bve in reg.Bven) {
            await UpsertBveAsync(db, entity.Id, bve, token);
        }
    }

    // =====================================================================
    // BVE
    // =====================================================================
    private async Task UpsertBveAsync(
        UjBauDbContext    db,
        long              regRef,
        BbpNeoBve         dto,
        CancellationToken token) {
        var bve = await db.BbpneoMassnahmeRegelungBve
            .FirstOrDefaultAsync(
                x => x.BbpneoMasRegRef == regRef &&
                     x.BveId           == dto.BveId,
                token);

        if (bve == null) {
            bve = new BbpneoMassnahmeRegelungBve {
                BbpneoMasRegRef = regRef,
                BveId           = dto.BveId
            };
            db.Add(bve);
        }

        bve.Aktiv            = dto.Aktiv;
        bve.Art              = dto.Art;
        bve.OrtMikroskopisch = dto.OrtMikroskopisch;
        bve.Bemerkung        = dto.Bemerkung;
        bve.GueltigkeitVon   = dto.GueltigkeitVon;
        bve.GueltigkeitBis   = dto.GueltigkeitBis;
        bve.Gueltigkeit      = dto.Gueltigkeit;
        bve.GueltigkeitEffektiveVerkehrstage =
            dto.GueltigkeitEffektiveVerkehrstage;

        if (!string.IsNullOrWhiteSpace(dto.VonBstDs100) && dto.VonVzG.HasValue) {
            var bst = await _resolver.ResolveOrCreateBetriebsstelleAsync(db, dto.VonBstDs100, token);

            if (bst > 0) {
                bve.Bst2strVonRef = await _resolver.ResolveBst2StrAsync(db, bst, dto.VonVzG.Value, token: token);
            }
        }

        if (!string.IsNullOrWhiteSpace(dto.BisBstDs100) && dto.BisVzG.HasValue) {
            var bst =
                await _resolver.ResolveOrCreateBetriebsstelleAsync(db, dto.BisBstDs100, token);

            if (bst > 0) {
                bve.Bst2strBisRef = await _resolver.ResolveBst2StrAsync(db, bst, dto.BisVzG.Value, token: token);
            }
        }

        bve.ApsBetroffenheit     = dto.Aps != null;
        bve.ApsBeschreibung      = dto.Aps?.Beschreibung;
        bve.ApsFreiVonFahrzeugen = dto.Aps?.FreiVonFahrzeugen ?? false;
        bve.IavBetroffenheit = dto.Iav != null;
        bve.IavBeschreibung  = dto.Iav?.Beschreibung;

        await db.SaveChangesAsync(token);

        if (dto.Aps != null) {
            await UpsertApsAsync(db, bve.Id, dto.Aps, token);
        }

        if (dto.Iav != null) {
            foreach (var iav in dto.Iav.Betroffenheiten) {
                await UpsertIavAsync(db, bve.Id, iav, token);
            }
        }
    }

    // =====================================================================
    // APS
    // =====================================================================
    private async Task UpsertApsAsync(
        UjBauDbContext    db,
        long              bveRef,
        BbpNeoAps         aps,
        CancellationToken token) {
        foreach (var dto in aps.Betroffenheiten) {
            if (string.IsNullOrWhiteSpace(dto.Uuid))
                continue;

            var entity = await db.BbpneoMassnahmeRegelungBveAps
                .FirstOrDefaultAsync(
                    x => x.BbpneoMassnahmeRegelungBveRef == bveRef &&
                         x.Uuid                          == dto.Uuid,
                    token);

            if (entity == null) {
                entity = new BbpneoMassnahmeRegelungBveAps {
                    BbpneoMassnahmeRegelungBveRef = bveRef,
                    Uuid                          = dto.Uuid
                };
                db.Add(entity);
            }

            entity.Gleis                         = dto.Gleis;
            entity.PrimaereKategorie             = dto.PrimaereKat;
            entity.SekundaereKategorie           = dto.SekundaerKat ?? string.Empty;
            entity.Oberleitung                   = dto.Oberleitung;
            entity.OberleitungAus                = dto.OberleitungAus;
            entity.TechnischerPlatz              = dto.TechnischerPlatz;
            entity.ArtDerAnbindung               = dto.ArtDerAnbindung;
            entity.EinschraenkungBefahrbarkeitSe = dto.EinschraenkungBefahrbarkeitSe;
            entity.Kommentar                     = dto.Kommentar;
            entity.AbFahrplanjahr                = dto.AbFahrplanjahr;
            entity.BstRef                        = await _resolver.ResolveOrCreateBetriebsstelleAsync(db, dto.BstDs100, token);

            entity.MoeglicheZa = dto.MoeglicheZAs is { Count: > 0 }
                    ? JsonSerializer.Serialize(
                        dto.MoeglicheZAs,
#pragma warning disable CA1869
                        new JsonSerializerOptions {
#pragma warning restore CA1869
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        })
                    : string.Empty;
            
            await db.SaveChangesAsync(token);
        }
    }

    // =====================================================================
    // IAV
    // =====================================================================
    private async Task UpsertIavAsync(
        UjBauDbContext         db,
        long                   bveRef,
        BbpNeoIavBetroffenheit dto,
        CancellationToken      token) {
        if (string.IsNullOrWhiteSpace(dto.VertragNr))
            return;

        var entity = await db.BbpneoMassnahmeRegelungBveIav
            .FirstOrDefaultAsync(
                x => x.BbpneoMassnahmeRegelungBveRef == bveRef &&
                     x.VertragNr                     == dto.VertragNr,
                token);

        if (entity == null) {
            entity = new BbpneoMassnahmeRegelungBveIav {
                BbpneoMassnahmeRegelungBveRef = bveRef,
                VertragNr                     = dto.VertragNr
            };
            db.Add(entity);
        }

        var bstRef = await _resolver.ResolveOrCreateBetriebsstelleAsync(db, dto.BstDs100, token);
        var strRef = await _resolver.ResolveStreckeAsync(db, dto.VzgStrecke);

        entity.Kunde                         = dto.Kunde;
        entity.Anschlussgrenze               = dto.Anschlussgrenze;
        entity.Oberleitung                   = dto.Oberleitung    ?? false;
        entity.OberleitungAus                = dto.OberleitungAus ?? false;
        entity.EinschraenkungBedienbarkeitIa = dto.EinschraenkungBedienbarkeitIA;
        entity.Kommentar                     = dto.Kommentar;
        entity.Bst2strRef                    = await _resolver.ResolveBst2StrAsync(db, bstRef, strRef, token: token);
        entity.VertragArt                    = dto.VertragArt;
        entity.VertragStatus                 = dto.VertragStatus;

        await db.SaveChangesAsync(token);
    }
}