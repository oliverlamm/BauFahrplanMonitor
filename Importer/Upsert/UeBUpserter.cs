using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Helpers;
using BauFahrplanMonitor.Importer.Dto.Ueb;
using BauFahrplanMonitor.Importer.Helper;
using BauFahrplanMonitor.Importer.Interface;
using BauFahrplanMonitor.Importer.Mapper;
using BauFahrplanMonitor.Models;
using BauFahrplanMonitor.Resolver;
using BauFahrplanMonitor.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NLog;
using Npgsql;

namespace BauFahrplanMonitor.Importer.Upsert;

public class UeBUpserter(
    IDbContextFactory<UjBauDbContext> dbFactory,
    SharedReferenceResolver           resolver,
    ConfigService                     config)
    : ImportUpserterBase(
            config,
            LogManager.GetCurrentClassLogger()),
        IUeBUpserter {
    private static readonly Logger         Logger = LogManager.GetCurrentClassLogger();
    public                  UeBImportStats Stats  = null!;

    // =====================================================================
    // ENTRYPOINT mit Progress
    // =====================================================================
    public async Task<UpsertResult> UpsertAsync(
        UjBauDbContext                db,
        UebXmlDocumentDto             dto,
        IProgress<UpsertProgressInfo> progress,
        CancellationToken             token) {
        token.ThrowIfCancellationRequested();

        // -------------------------------------------------
        // Statistik initialisieren
        // -------------------------------------------------
        Stats = new UeBImportStats {
            Dokumente = 1
        };

        var sw = Stopwatch.StartNew();

        // -------------------------------------------------
        // PHASE A: Referenzen (kurz, sichtbar)
        // -------------------------------------------------
        var vorgangRef =
            await resolver.ResolveOrCreateVorgangAsync(db, dto.Vorgang, ImportMode.UeB, token);

        var senderRef =
            await resolver.ResolveOrCreateSenderAsync(db, dto.Header, token);

        // 🔑 Sichtbarkeit für andere Threads
        await db.SaveChangesAsync(token);

        // -------------------------------------------------
        // PHASE B: Datei-Inhalte (TX)
        // -------------------------------------------------
        IDbContextTransaction? tx = null;

        try {
            tx = await db.Database.BeginTransactionAsync(token);

            var dokumentRef =
                await ResolveOrInsertDokumentAsync(db, dto, vorgangRef, senderRef, token);

            await UpsertStreckenabschnitteAsync(dto, token, db, dokumentRef);

            // -------------------------------------------------
            // FACTORY: Züge + Regelungen kanonisieren
            // -------------------------------------------------
            var factoryResult =
                UebZugFactory.Build(dto.Document);

            // Factory-Statistik übernehmen
            Stats.SevsGelesen              += factoryResult.SevsGelesen;
            Stats.SevsMitErsatzzug         += factoryResult.SevsMitErsatzzug;
            Stats.ZuegeAusSevErzeugt       += factoryResult.ZuegeAusSevErzeugt;
            Stats.ErsatzzuegeAusSevErzeugt += factoryResult.ErsatzzuegeAusSevErzeugt;

            // -------------------------------------------------
            // ZÜGE + Knotenzeiten + Regelungen (Bulk)
            // -------------------------------------------------
            db.ChangeTracker.AutoDetectChangesEnabled = false;

            var totalZuege = factoryResult.Zuege.Count;
            var index      = 0;

            foreach (var zug in factoryResult.Zuege) {
                token.ThrowIfCancellationRequested();
                index++;

                if (index % 5 == 0 || index == totalZuege) {
                    progress?.Report(new UpsertProgressInfo {
                        Phase   = UpsertPhase.Zuege,
                        Current = index,
                        Total   = totalZuege
                    });
                }

                // -----------------------------
                // Zug upserten
                // -----------------------------
                var zugRef = await ResolveOrInsertZugAsync(db, zug, dokumentRef, dto, token);

                // 1️⃣ Abschnitts-Regelung (IMMER!)
                await UpsertFploAbschnittRegelungAsync(db, zug, zugRef, token);

                // 2️⃣ Knotenzeiten
                await UpsertKnotenzeitenAsync(db, zug, zugRef, token);

                // 3️⃣ Detail-Regelungen aus Factory (SEV, Ausfall_von/bis, etc.)
                var key = new UebZugKey(zug.Zugnummer, zug.Verkehrstag);

                if (!factoryResult.Regelungen.TryGetValue(key, out var regs)) continue;
                foreach (var reg in regs) {
                    await UpsertRegelungAsync(db, reg, zugRef, token);
                }
            }

            db.ChangeTracker.AutoDetectChangesEnabled = true;

            await db.SaveChangesAsync(token);
            await tx.CommitAsync(token);
            db.ChangeTracker.Clear();

            sw.Stop();

            Logger.Info("ÜB-Import abgeschlossen: Datei='{0}', Dauer={1}, Stats: {2}", dto.Document.Dateiname,
                sw.Elapsed, Stats);

            return new UpsertResult {
                DokumentRef = dokumentRef,
                UeBStats    = Stats
            };
        }
        catch (OperationCanceledException) {
            if (tx != null)
                await tx.RollbackAsync(CancellationToken.None);

            throw;
        }
        catch (Exception ex) {
            if (tx != null)
                await tx.RollbackAsync(CancellationToken.None);

            HandleException(ex, "UeBUpsert", new {
                dto.Document.Dateiname,
                dto.Vorgang.MasterFplo
            });

            throw;
        }
        finally {
            if (tx != null)
                await tx.DisposeAsync();
        }
    }

    // =====================================================================
    // Regelungen
    // =====================================================================
    private async Task UpsertRegelungAsync(
        UjBauDbContext    db,
        UebRegelungDto    regelung,
        long              zugRef,
        CancellationToken token) {
        token.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(regelung.Art))
            throw new InvalidOperationException("Regelung ohne Art");

        if (string.IsNullOrWhiteSpace(regelung.AnchorRl100))
            throw new InvalidOperationException(
                $"Regelung '{regelung.Art}' ohne Anchor");

        // -------------------------------------------------
        // Anchor → Betriebsstelle auflösen
        // -------------------------------------------------
        var ankerBstRef =
            await resolver.ResolveOrCreateBetriebsstelleAsync(
                db,
                regelung.AnchorRl100,
                token);

        // -------------------------------------------------
        // Existiert die Regelung bereits?
        // -------------------------------------------------
        var exists =
            await db.UebDokumentZugRegelung.AnyAsync(
                r =>
                    r.UebDokumentZugRef == zugRef       &&
                    r.Art               == regelung.Art &&
                    r.AnkerBstRef       == ankerBstRef,
                token);

        if (exists)
            return;

        // -------------------------------------------------
        // RegelungJson normalisieren (NULL oder valides JSON)
        // -------------------------------------------------
        string? json = NormalizeRegelungJson(
            regelung.JsonRaw,
            regelung.Art,
            zugRef);

        // -------------------------------------------------
        // Insert
        // -------------------------------------------------
        db.UebDokumentZugRegelung.Add(
            new UebDokumentZugRegelung {
                UebDokumentZugRef = zugRef,
                Art               = regelung.Art,
                AnkerBstRef       = ankerBstRef,
                Regelung          = json // ✅ NULL oder valides JSON
            });

        Stats.RegelungenInserted++;
    }

    // =====================================================================
    // Knotenzeiten
    // =====================================================================
    private async Task UpsertKnotenzeitenAsync(
        UjBauDbContext    db,
        UebZugDto         zug,
        long              zugRef,
        CancellationToken token) {
        if (zug.Knotenzeiten is not { Count: > 0 })
            return;

        // -------------------------------------------------
        // Alte Knotenzeiten entfernen
        // -------------------------------------------------
        await db.UebDokumentZugKnotenzeiten
            .Where(x => x.UebDokumentZugRef == zugRef)
            .ExecuteDeleteAsync(token);

        // -------------------------------------------------
        // Sortierung: RelativLage, sonst Reihenfolge
        // -------------------------------------------------
        var ordered = zug.Knotenzeiten
            .OrderBy(k => k.RelativLage)
            .ThenBy(k => k.AnkunftsZeit ?? DateTime.MaxValue)
            .ToList();

        long lfdnr = 0;

        foreach (var k in ordered) {
            token.ThrowIfCancellationRequested();
            lfdnr++;

            long? bstRef = null;

            if (!string.IsNullOrWhiteSpace(k.BahnhofDs100)) {
                bstRef = await resolver.ResolveOrCreateBetriebsstelleAsync(db, k.BahnhofDs100, token);
            }

            db.UebDokumentZugKnotenzeiten.Add(
                new UebDokumentZugKnotenzeiten {
                    UebDokumentZugRef = zugRef,
                    Lfdnr             = lfdnr,
                    Relativlage       = k.RelativLage,
                    BstRef            = bstRef,
                    Ankunft           = k.AnkunftsZeit,
                    Abfahrt           = k.Abfahrtszeit,
                    Haltart           = k.Haltart,
                    Bemerkung         = null
                });

            // ✅ Statistik: jede neu geschriebene Knotenzeit
            Stats.KnotenzeitenInserted++;
        }
    }

    // =====================================================================
    // Streckenabschnitte
    // =====================================================================
    private static async Task UpsertStreckenabschnitteAsync(
        UebXmlDocumentDto dto,
        CancellationToken token,
        UjBauDbContext    db,
        long              dokumentRef) {
        if (dto.Document.Strecken is { Count: > 0 }) {
            // Alte Einträge für dieses Dokument entfernen
            // (Dokument ist die einzige Identität)
            await db.UebDokumentStreckenabschnitte
                .Where(x => x.UebDokumentRef == dokumentRef)
                .ExecuteDeleteAsync(token);

            var uniqueStrecken = dto.Document.Strecken
                .GroupBy(s => new {
                    s.StartBst,
                    s.EndBst,
                    s.Massnahme,
                    s.Betriebsweise,
                    s.Grund,
                    s.Baubeginn,
                    s.Bauende
                })
                .Select(g => g.First())
                .ToList();

            foreach (var strecke in uniqueStrecken) {
                token.ThrowIfCancellationRequested();

                db.UebDokumentStreckenabschnitte.Add(
                    new UebDokumentStreckenabschnitte {
                        UebDokumentRef = dokumentRef,
                        StartBstRl100  = strecke.StartBst      ?? string.Empty,
                        EndBstRl100    = strecke.EndBst        ?? string.Empty,
                        Massnahme      = strecke.Massnahme     ?? string.Empty,
                        Betriebsweise  = strecke.Betriebsweise ?? string.Empty,
                        Grund          = strecke.Grund         ?? string.Empty,
                        Baubeginn      = strecke.Baubeginn.Value,
                        Bauende        = strecke.Bauende.Value
                    });
            }
        }
    }

    // =====================================================================
    // Züge
    // =====================================================================
    private async Task<long> ResolveOrInsertZugAsync(
        UjBauDbContext    db,
        UebZugDto         zug,
        long              dokumentRef,
        UebXmlDocumentDto dto,
        CancellationToken token) {
        token.ThrowIfCancellationRequested();

        if (zug.Zugnummer <= 0 || zug.Verkehrstag == default)
            throw new InvalidOperationException(
                $"Ungültiger Zug: ZugNr={zug.Zugnummer}, VT={zug.Verkehrstag}");

        // -------------------------------------------------
        // Betreiber / Kunde auflösen (inkl. Header-Fallback)
        // -------------------------------------------------
        long kundeRef;

        if (!string.IsNullOrWhiteSpace(zug.Betreiber)) {
            kundeRef = await resolver.ResolveOrCreateKundeAsync(
                db,
                zug.Betreiber,
                token);
        }
        else {
            var headerKundeRef =
                await TryResolveKundeFromEmpfaengerAsync(db, dto, token);

            if (headerKundeRef.HasValue) {
                kundeRef = headerKundeRef.Value;
                Stats.SevKundeRefFallbackHeader++;
            }
            else {
                kundeRef = 0;
                Stats.SevKundeRefFallbackZero++;

                Logger.Warn(
                    "SEV-Zug ohne Betreiber (kein Header-Fallback): ZugNr={0}, Verkehrstag={1}",
                    zug.Zugnummer,
                    zug.Verkehrstag);
            }
        }

        // -------------------------------------------------
        // Existierenden Zug suchen
        // -------------------------------------------------
        var existing =
            await db.UebDokumentZug
                .FirstOrDefaultAsync(
                    z =>
                        z.Zugnr       == zug.Zugnummer &&
                        z.Verkehrstag == zug.Verkehrstag,
                    token);

        // =================================================
        // INSERT
        // =================================================
        if (existing == null) {
            var entity = new UebDokumentZug {
                UebDokumentRef = dokumentRef,
                Zugnr          = zug.Zugnummer,
                Verkehrstag    = zug.Verkehrstag,
                KundeRef       = kundeRef,
                Bedarf         = zug.Bedarf,
            };

            db.UebDokumentZug.Add(entity);

            Stats.ZuegeInserted++;

            return entity.Id;
        }

        // =================================================
        // UPDATE (nur wenn sich etwas ändert)
        // =================================================
        var changed = false;

        void Set<T>(T current, T next, Action<T> setter) {
            if (EqualityComparer<T>.Default.Equals(current, next)) return;
            setter(next);
            changed = true;
        }

        Set(existing.KundeRef, kundeRef,   v => existing.KundeRef = v);
        Set(existing.Bedarf,   zug.Bedarf, v => existing.Bedarf   = v);

        if (changed) {
            Stats.ZuegeUpdated++;
        }

        return existing.Id;
    }


    // =====================================================================
    // Exit
    // =====================================================================
    public async Task MarkImportCompletedAsync(long uebDokumentRef, CancellationToken token) {
        await using var db = await dbFactory.CreateDbContextAsync(token);

        var doc = await db.UebDokument.FindAsync([uebDokumentRef], token);
        if (doc == null) return;

        doc.ImportTimestamp = DateTime.Now;

        await db.SaveChangesAsync(token);
    }

    // =====================================================================
    // Dokumente
    // =====================================================================
    private async Task<long> ResolveOrInsertDokumentAsync(
        UjBauDbContext    db,
        UebXmlDocumentDto dto,
        long              vorgangRef,
        long?             senderRef,
        CancellationToken token) {
        // -------------------------------------------------
        // Fast Lookup (read-only)
        // -------------------------------------------------
        var existing = await db.UebDokument
            .AsNoTracking()
            .FirstOrDefaultAsync(d =>
                    d.UjbauVorgangRef == vorgangRef &&
                    d.Dateiname       == dto.Document.Dateiname,
                token);

        if (existing != null)
            return existing.Id;

        // -------------------------------------------------
        // Region auflösen
        // -------------------------------------------------
        if (string.IsNullOrWhiteSpace(dto.Document.Masterniederlassung))
            throw new InvalidOperationException("Masterniederlassung fehlt");

        var regionRef = await resolver.ResolveRegionAsync(db, dto.Document.Masterniederlassung, token);

        if (regionRef <= 0)
            throw new InvalidOperationException(
                $"Region '{dto.Document.Masterniederlassung}' konnte nicht aufgelöst werden");

        // -------------------------------------------------
        // Create
        // -------------------------------------------------
        var doc = new UebDokument {
            UjbauVorgangRef = vorgangRef,
            SenderRef       = senderRef ?? throw new InvalidOperationException("SenderRef fehlt"),
            ExportTimestamp = dto.Header.Timestamp,

            VersionMajor = dto.Document.Version.Major,
            VersionMinor = dto.Document.Version.Minor,
            VersionSub   = dto.Document.Version.Sub,
            Version      = dto.Document.Version.VersionNumeric ?? 0,

            RegionRef      = regionRef,
            GueltigkeitVon = dto.Document.GueltigAb,
            GueltigkeitBis = dto.Document.GueltigBis,
            Allgemein      = dto.Document.AllgemeinText,
            Dateiname      = dto.Document.Dateiname,
        };

        db.UebDokument.Add(doc);

        try {
            await db.SaveChangesAsync(token);
            return doc.Id;
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex)) {
            var winnerId = await db.UebDokument
                .Where(d =>
                    d.UjbauVorgangRef == vorgangRef &&
                    d.Dateiname       == dto.Document.Dateiname)
                .Select(d => d.Id)
                .SingleOrDefaultAsync(token);

            if (winnerId > 0)
                return winnerId;

            throw new InvalidOperationException(
                "ZvfDokument konnte nach UniqueViolation nicht erneut gelesen werden", ex);
        }
    }

    // =====================================================================
    // Helper
    // =====================================================================
    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

    private static bool SetIfDifferent<T>(T current, T next, Action<T> setter) {
        if (EqualityComparer<T>.Default.Equals(current, next))
            return false;

        setter(next);
        return true;
    }

    private static string? NormalizeRegelungJson(
        string? rawJson,
        string  regelungsArt,
        long    zugRef) {
        // NULL oder Whitespace → NULL
        if (string.IsNullOrWhiteSpace(rawJson))
            return null;

        try {
            // Validierung: einmal parsen reicht
            JsonDocument.Parse(rawJson);
            return rawJson;
        }
        catch (JsonException ex) {
            Logger.Error(
                ex,
                "Ungültiges Regelung-JSON verworfen | ZugRef={0} | Art={1} | Raw={2}",
                zugRef,
                regelungsArt,
                rawJson);

            // harte Regel: ungültiges JSON wird nicht persistiert
            return null;
        }
    }

    private async Task<long?> TryResolveKundeFromEmpfaengerAsync(
        UjBauDbContext    db,
        UebXmlDocumentDto dto,
        CancellationToken token) {
        var empfaenger = dto.Header?.Empfaenger;

        if (empfaenger is not { Count: 1 })
            return null;

        var kbezRaw = empfaenger[0];

        Logger.Debug("Header.Empfaengerliste.Count={0}, Wert='{1}'",
            empfaenger?.Count ?? 0,
            empfaenger is { Count: 1 } ? empfaenger[0] : "");

        if (string.IsNullOrWhiteSpace(kbezRaw))
            return null;

        var kbez = kbezRaw.Trim();

        // Achtung: Mehrfachtreffer sind erlaubt → nimm kleinste Id als "ersten"
        var candidates = await db.BasisKunde
            .Where(k => k.Kbez == kbez)
            .Select(k => new { k.Id })
            .ToListAsync(token);

        if (candidates.Count == 0) {
            Logger.Warn("Header-Empfänger nicht in basis_kunde gefunden: Kbez='{0}'", kbez);
            return null;
        }

        var chosen = candidates.MinBy(x => x.Id)!.Id;

        Logger.Info(
            "SEV-Betreiber per Header-Empfänger aufgelöst: Kbez='{0}' → KundeRef={1} (Treffer={2})",
            kbez,
            chosen,
            candidates.Count);

        return chosen;
    }

    private async Task UpsertFploAbschnittRegelungAsync(
        UjBauDbContext    db,
        UebZugDto         zug,
        long              zugRef,
        CancellationToken token) {
        token.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(zug.FploAbschnitt))
            return;

        // -------------------------------------------------
        // Anchor bestimmen
        // -------------------------------------------------
        string? anchorDs100 = zug.FploAbschnitt switch {
            // Zustand / Attribute mit Knoten-Anker
            "Umleitung" or
                "SEV" or
                "Vorplanfahrt" or
                "Zusätzliche Leistungen" or
                "Verspätung auf Regelweg"
                => zug.Knotenzeiten?
                    .OrderBy(k => k.RelativLage)
                    .FirstOrDefault()
                    ?.BahnhofDs100,

            // Gesamtausfall
            "Ausfall"
                => zug.Regelweg?.Abgangsbahnhof?.Ds100,

            _ => null
        };

        if (string.IsNullOrWhiteSpace(anchorDs100)) {
            Logger.Warn(
                "FploAbschnitt ohne Anchor | Zug={0}/{1} | Abschnitt={2}",
                zug.Zugnummer,
                zug.Verkehrstag,
                zug.FploAbschnitt);
            return;
        }

        var ankerBstRef =
            await resolver.ResolveOrCreateBetriebsstelleAsync(
                db,
                anchorDs100,
                token);

        var kategorie = GetKategorie(zug.FploAbschnitt);

        // -------------------------------------------------
        // Existenzprüfung
        // -------------------------------------------------
        if (kategorie == RegelungsKategorie.Zustand) {
            // ❗ Zustand: exklusiv pro Anchor
            var existsZustand =
                await db.UebDokumentZugRegelung.AnyAsync(
                    r =>
                        r.UebDokumentZugRef == zugRef      &&
                        r.AnkerBstRef       == ankerBstRef &&
                        GetKategorie(r.Art) == RegelungsKategorie.Zustand,
                    token);

            if (existsZustand)
                return;
        }
        else {
            // ➕ Attribut: nur identische Regelung verhindern
            var existsSame =
                await db.UebDokumentZugRegelung.AnyAsync(
                    r =>
                        r.UebDokumentZugRef == zugRef      &&
                        r.AnkerBstRef       == ankerBstRef &&
                        r.Art               == zug.FploAbschnitt,
                    token);

            if (existsSame)
                return;
        }

        // -------------------------------------------------
        // Insert (ohne JSON!)
        // -------------------------------------------------
        db.UebDokumentZugRegelung.Add(
            new UebDokumentZugRegelung {
                UebDokumentZugRef = zugRef,
                Art               = zug.FploAbschnitt,
                AnkerBstRef       = ankerBstRef,
                Regelung          = null
            });

        Stats.RegelungenInserted++;
    }


    private static RegelungsKategorie GetKategorie(string art) =>
        art switch {
            "Ausfall"   => RegelungsKategorie.Zustand,
            "Umleitung" => RegelungsKategorie.Zustand,
            "SEV"       => RegelungsKategorie.Zustand,

            "Vorplanfahrt"            => RegelungsKategorie.Attribut,
            "Zusätzliche Leistungen"  => RegelungsKategorie.Attribut,
            "Verspätung auf Regelweg" => RegelungsKategorie.Attribut,

            _ => RegelungsKategorie.Attribut
        };
}