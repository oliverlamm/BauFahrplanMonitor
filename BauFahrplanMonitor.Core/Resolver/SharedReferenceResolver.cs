using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using BauFahrplanMonitor.Core.Data;
using BauFahrplanMonitor.Core.Helpers;
using BauFahrplanMonitor.Core.Importer.Dto.Shared;
using BauFahrplanMonitor.Core.Interfaces;
using BauFahrplanMonitor.Core.Models;
using Microsoft.EntityFrameworkCore;
using NLog;
using Npgsql;

namespace BauFahrplanMonitor.Core.Resolver;

/// <summary>
/// Zentrale, threadsichere Referenzauflösung.
/// Alle CREATE-Operationen laufen in eigenen DbContexts.
/// </summary>
public sealed class SharedReferenceResolver(IDbContextFactory<UjBauDbContext> dbFactory) {

    private static readonly Logger Logger = LogManager.GetLogger("SharedReferenceResolver");

    // ==========================================================
    // LOCKS (prozesslokal)
    // ==========================================================
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    private SemaphoreSlim GetLock(string scope, string key)
        => _locks.GetOrAdd($"{scope}:{key}", _ => new SemaphoreSlim(1, 1));

    // ==========================================================
    // CACHES (NUR committed IDs!)
    // ==========================================================
    private readonly ConcurrentDictionary<string, long>      _bstCache     = new();
    private readonly ConcurrentDictionary<string, long>      _kundeCache   = new();
    private readonly ConcurrentDictionary<long, long>        _streckeCache = new();
    private readonly ConcurrentDictionary<string, long>      _bst2StrCache = new();
    private readonly ConcurrentDictionary<string, long>      _regionCache  = new();
    private readonly ConcurrentDictionary<(long, int), long> _vorgangCache = new();
    private readonly ConcurrentDictionary<(string Name, string Vorname, string Mail), long>
        _senderCache = new();
    private readonly ConcurrentDictionary<(long VorgangRef, string Bbmn), byte>
        _bbmnCache = new();

    // ==========================================================
    // BETRIEBSSTELLE
    // ==========================================================
    public async Task<long> ResolveOrCreateBetriebsstelleAsync(
        UjBauDbContext    _,
        string?           raw,
        CancellationToken token) {
        var key = Ds100Normalizer.Clean(raw)?.ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(key))
            return 0; // FK-safe

        if (_bstCache.TryGetValue(key, out var cached))
            return cached;

        var sem = GetLock("BST", key);
        await sem.WaitAsync(token);

        try {
            if (_bstCache.TryGetValue(key, out cached))
                return cached;

            await using var ctx = await dbFactory.CreateDbContextAsync(token);

            var id = await ctx.BasisBetriebsstelle
                .AsNoTracking()
                .Where(b => b.Rl100 != null && b.Rl100.ToUpper() == key)
                .Select(b => b.Id)
                .FirstOrDefaultAsync(token);

            if (id > 0) {
                _bstCache[key] = id;
                return id;
            }

            var bst = new BasisBetriebsstelle {
                Rl100             = key,
                NetzbezirkRef     = 0,
                TypRef            = 0,
                RegionRef         = 0,
                Zustand           = "in Betrieb",
                IstBasisDatensatz = false
            };

            ctx.BasisBetriebsstelle.Add(bst);

            try {
                await ctx.SaveChangesAsync(token);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex)) {
                id = await ctx.BasisBetriebsstelle
                    .AsNoTracking()
                    .Where(b => b.Rl100 != null && b.Rl100.ToUpper() == key)
                    .Select(b => b.Id)
                    .FirstAsync(token);

                _bstCache[key] = id;
                return id;
            }

            _bstCache[key] = bst.Id;
            Logger.Warn("[Bst] angelegt: {0} → {1}", key, bst.Id);

            return bst.Id;
        }
        finally {
            sem.Release();
        }
    }

    // =====================================================================
    // BETRIEBSSTELLE (SMART)
    // =====================================================================
    public async Task<long> ResolveOrCreateBetriebsstelleSmartAsync(
        UjBauDbContext    db,
        string?           ds100,
        string?           nameFallback,
        string            context,
        long?             zugNr,
        DateOnly?         verkehrstag,
        CancellationToken token) {
        // 1) Primär: DS100
        if (!string.IsNullOrWhiteSpace(ds100)) {
            var key = Ds100Normalizer.Clean(ds100);
            if (!string.IsNullOrWhiteSpace(key)) {
                var id = await ResolveOrCreateBetriebsstelleAsync(db, key, token);
                if (id > 0)
                    return id;
            }
        }

        // 2) Fallback: Name
        if (!string.IsNullOrWhiteSpace(nameFallback)) {
            var key = Ds100Normalizer.Clean(nameFallback);
            if (!string.IsNullOrWhiteSpace(key)) {
                var id = await ResolveOrCreateBetriebsstelleAsync(db, key, token);
                if (id > 0) {
                    Logger.Info(
                        "[Bst.ResolveSmart] Fallback über Name | Context={Context}, '{Name}'",
                        context,
                        nameFallback);
                    return id;
                }
            }
        }

        // 3) Logging mit Fachkontext
        Logger.Warn(
            "[Bst.ResolveSmart] Nicht auflösbar | Context={Context}, Zug={Zug}/{Tag}, ds100='{Ds100}', name='{Name}'",
            context,
            zugNr?.ToString()                   ?? "-",
            verkehrstag?.ToString("yyyy-MM-dd") ?? "-",
            ds100                               ?? "",
            nameFallback                        ?? ""
        );

        return 0; // FK-safe
    }

    // =====================================================================
    // BETRIEBSSTELLE (CACHED WRAPPER)
    // =====================================================================
    public async Task<long> ResolveOrCreateBetriebsstelleCachedAsync(
        UjBauDbContext    db,
        string?           raw,
        CancellationToken token) {
        var key = Ds100Normalizer.Clean(raw);

        if (string.IsNullOrWhiteSpace(key))
            return 0; // 🔑 FK-safe

        // --------------------------------------------------
        // Cache nur lesen (niemals schreiben!)
        // --------------------------------------------------
        if (_bstCache.TryGetValue(key, out var cached) && cached > 0)
            return cached;

        // --------------------------------------------------
        // Delegation an die saubere Core-Methode
        // --------------------------------------------------
        var id = await ResolveOrCreateBetriebsstelleAsync(db, key, token);

        // --------------------------------------------------
        // Cache NUR, wenn Datensatz bereits existierte
        // (ResolveOrCreateBetriebsstelleAsync cached selbst
        // nur bei DB-Treffern)
        // --------------------------------------------------
        return id;
    }

    // ==========================================================
    // KUNDE
    // ==========================================================
    public async Task<long> ResolveOrCreateKundeAsync(
        UjBauDbContext    _,
        string?           kundeCode,
        CancellationToken token) {
        var key = string.IsNullOrWhiteSpace(kundeCode)
            ? "UNKNOWN"
            : kundeCode.Trim();

        if (_kundeCache.TryGetValue(key, out var cached))
            return cached;

        var sem = GetLock("KUNDE", key);
        await sem.WaitAsync(token);

        try {
            if (_kundeCache.TryGetValue(key, out cached))
                return cached;

            await using var ctx = await dbFactory.CreateDbContextAsync(token);

            var id = await ctx.BasisKunde
                .AsNoTracking()
                .Where(k => k.Kdnnr == key)
                .Select(k => k.Id)
                .FirstOrDefaultAsync(token);

            if (id > 0) {
                _kundeCache[key] = id;
                return id;
            }

            var kunde = new BasisKunde {
                Kdnnr = key
            };
            ctx.BasisKunde.Add(kunde);

            try {
                await ctx.SaveChangesAsync(token);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex)) {
                id = await ctx.BasisKunde
                    .AsNoTracking()
                    .Where(k => k.Kdnnr == key)
                    .Select(k => k.Id)
                    .FirstAsync(token);

                _kundeCache[key] = id;
                return id;
            }

            _kundeCache[key] = kunde.Id;
            Logger.Info("[Kunde] angelegt: {0} → {1}", key, kunde.Id);

            return kunde.Id;
        }
        finally {
            sem.Release();
        }
    }

    // ==========================================================
    // STRECKE (read-only)
    // ==========================================================
    public async Task<long> ResolveStreckeAsync(
        UjBauDbContext    db,
        long?             vzgNr,
        CancellationToken token = default) {
        if (vzgNr is null or <= 0)
            return 0;

        if (_streckeCache.TryGetValue(vzgNr.Value, out var cached))
            return cached;

        var id = await db.BasisStrecke
            .AsNoTracking()
            .Where(s => s.VzgNr == vzgNr)
            .Select(s => s.Id)
            .FirstOrDefaultAsync(token);

        _streckeCache[vzgNr.Value] = id;
        return id;
    }

    // ==========================================================
    // BETRIEBSSTELLE ↔ STRECKE
    // ==========================================================
    public async Task<long> ResolveBst2StrAsync(
        UjBauDbContext    _,
        long              bstRef,
        long              strRef,
        string?           kmL   = null,
        CancellationToken token = default) {
        var key = $"{bstRef}|{strRef}";

        if (_bst2StrCache.TryGetValue(key, out var cached))
            return cached;

        var sem = GetLock("BST2STR", key);
        await sem.WaitAsync(token);

        try {
            if (_bst2StrCache.TryGetValue(key, out cached))
                return cached;

            await using var ctx = await dbFactory.CreateDbContextAsync(token);

            var id = await ctx.BasisBetriebsstelle2strecke
                .AsNoTracking()
                .Where(x => x.BstRef == bstRef && x.StreckeRef == strRef)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(token);

            if (id > 0) {
                _bst2StrCache[key] = id;
                return id;
            }

            var neu = new BasisBetriebsstelle2strecke {
                BstRef            = bstRef,
                StreckeRef        = strRef,
                KmL               = string.IsNullOrWhiteSpace(kmL) ? null : kmL,
                IstBasisDatensatz = false
            };

            ctx.BasisBetriebsstelle2strecke.Add(neu);

            try {
                await ctx.SaveChangesAsync(token);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex)) {
                id = await ctx.BasisBetriebsstelle2strecke
                    .AsNoTracking()
                    .Where(x => x.BstRef == bstRef && x.StreckeRef == strRef)
                    .Select(x => x.Id)
                    .FirstAsync(token);

                _bst2StrCache[key] = id;
                return id;
            }

            _bst2StrCache[key] = neu.Id;
            return neu.Id;
        }
        finally {
            sem.Release();
        }
    }

    // ==========================================================
    // REGION (read-only)
    // ==========================================================
    public async Task<long> ResolveRegionAsync(
        UjBauDbContext    db,
        string?           raw,
        CancellationToken token) {
        var key = NormalizeRegionKey(raw);
        if (string.IsNullOrWhiteSpace(key))
            return 0;

        if (_regionCache.TryGetValue(key, out var cached))
            return cached;

        var id = await db.BasisRegion
            .AsNoTracking()
            .Where(r =>
                r.Bezeichner != null && r.Bezeichner.ToLower() == key ||
                r.Langname   != null && r.Langname.ToLower()   == key ||
                r.Kbez       != null && r.Kbez.ToLower()       == key)
            .Select(r => r.Id)
            .FirstOrDefaultAsync(token);

        _regionCache[key] = id;
        return id;
    }

    // ==========================================================
    // VORGANG 
    // ==========================================================
    public async Task<long> ResolveOrCreateVorgangAsync(
        UjBauDbContext    _,
        SharedVorgangDto  dto,
        ImportMode        importMode,
        CancellationToken token) {
        if (dto.MasterFplo <= 0 || !dto.FahrplanJahr.HasValue)
            throw new ArgumentException("Ungültiger Vorgang");

        var key = (dto.MasterFplo, dto.FahrplanJahr.Value);

        if (_vorgangCache.TryGetValue(key, out var cached))
            return cached;

        var sem = GetLock("VORGANG", $"{key.MasterFplo}:{dto.FahrplanJahr.Value}");
        await sem.WaitAsync(token);

        try {
            if (_vorgangCache.TryGetValue(key, out cached))
                return cached;

            await using var ctx = await dbFactory.CreateDbContextAsync(token);

            var vorgang = await ctx.UjbauVorgang
                .FirstOrDefaultAsync(v =>
                        v.VorgangNr    == dto.MasterFplo &&
                        v.Fahrplanjahr == dto.FahrplanJahr,
                    token);

            var isNew = false;

            if (vorgang == null) {
                vorgang = new UjbauVorgang {
                    VorgangNr    = dto.MasterFplo,
                    Fahrplanjahr = dto.FahrplanJahr,
                    Kategorie    = string.IsNullOrWhiteSpace(dto.Kategorie) ? "A" : dto.Kategorie
                };
                ctx.UjbauVorgang.Add(vorgang);
                isNew = true;
            }

            if (dto is IExtendedVorgangDto ext) {
                if (isNew || importMode != ImportMode.UeB)
                    ext.ApplyTo(vorgang);
                else
                    ext.ApplyIfEmptyTo(vorgang);
            }

            try {
                await ctx.SaveChangesAsync(token);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex)) {
                vorgang = await ctx.UjbauVorgang.SingleAsync(v =>
                        v.VorgangNr    == dto.MasterFplo &&
                        v.Fahrplanjahr == dto.FahrplanJahr,
                    token);
            }

            _vorgangCache[key] = vorgang.Id;
            return vorgang.Id;
        }
        finally {
            sem.Release();
        }
    }

    // ==========================================================
    // SENDER
    // ==========================================================
    public async Task<long> ResolveOrCreateSenderAsync(
        UjBauDbContext    _,
        SharedHeaderDto   header,
        CancellationToken token) {
        var key = (
            Name: (header.SenderName).Trim().ToLowerInvariant(),
            Vorname: (header.SenderVorname).Trim().ToLowerInvariant(),
            Mail: (header.SenderMail).Trim().ToLowerInvariant()
        );

        if (_senderCache.TryGetValue(key, out var cached))
            return cached;

        var sem = GetLock("SENDER", $"{key.Name}|{key.Vorname}|{key.Mail}");
        await sem.WaitAsync(token);

        try {
            if (_senderCache.TryGetValue(key, out cached))
                return cached;

            await using var ctx = await dbFactory.CreateDbContextAsync(token);

            var sender = await ctx.UjbauSender.FirstOrDefaultAsync(s =>
                    (s.Name    ?? "").ToLower() == key.Name    &&
                    (s.Vorname ?? "").ToLower() == key.Vorname &&
                    (s.Email   ?? "").ToLower() == key.Mail,
                token);

            if (sender == null) {
                sender = new UjbauSender {
                    Name      = header.SenderName.Trim(),
                    Vorname   = header.SenderVorname.Trim(),
                    Email     = header.SenderMail.Trim(),
                    Abteilung = header.SenderAbteilung,
                    Telefon   = header.SenderTelefon,
                    Strasse   = header.SenderAdresse,
                    Stadt     = header.SenderStadt,
                    Plz       = header.SenderPlz
                };
                ctx.UjbauSender.Add(sender);

                try {
                    await ctx.SaveChangesAsync(token);
                }
                catch (DbUpdateException ex) when (IsUniqueViolation(ex)) {
                    sender = await ctx.UjbauSender.SingleAsync(s =>
                            (s.Name    ?? "").ToLower() == key.Name    &&
                            (s.Vorname ?? "").ToLower() == key.Vorname &&
                            (s.Email   ?? "").ToLower() == key.Mail,
                        token);
                }
            }

            _senderCache[key] = sender.Id;
            return sender.Id;
        }
        finally {
            sem.Release();
        }
    }


    // ==========================================================
    // HELPER
    // ==========================================================
    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

    private static string NormalizeRegionKey(string? raw) {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        var s = raw.Trim().ToLowerInvariant();
        if (s.StartsWith("rb "))
            s = s[3..];

        s = Regex.Replace(s, @"\s+", " ");
        return s;
    }

    // =====================================================================
    // BBMN (In-Memory Dedup pro Importlauf)
    // =====================================================================
    public bool TryRegisterBbmn(long vorgangRef, string bbmn) {
        if (vorgangRef <= 0)
            return false;

        return !string.IsNullOrWhiteSpace(bbmn) && _bbmnCache.TryAdd((vorgangRef, bbmn.Trim()), 0);

    }

    // =====================================================================
    // Befüllen des RegionCache
    // =====================================================================
    public async Task WarmUpRegionCacheAsync(
        UjBauDbContext    db,
        CancellationToken token) {
        var regions = await db.BasisRegion
            .AsNoTracking()
            .Select(r => new {
                r.Id,
                r.Kbez,
                r.Bezeichner,
                r.Langname
            })
            .ToListAsync(token);

        foreach (var r in regions) {
            if (!string.IsNullOrWhiteSpace(r.Kbez))
                _regionCache.TryAdd(NormalizeRegionKey(r.Kbez), r.Id);

            if (!string.IsNullOrWhiteSpace(r.Bezeichner))
                _regionCache.TryAdd(NormalizeRegionKey(r.Bezeichner), r.Id);

            if (!string.IsNullOrWhiteSpace(r.Langname))
                _regionCache.TryAdd(NormalizeRegionKey(r.Langname), r.Id);
        }

        Logger.Info($"[Region.Cache] Warm-Up abgeschlossen: {_regionCache.Count} Einträge");
    }
}