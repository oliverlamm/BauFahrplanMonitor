using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using BauFahrplanMonitor.Core.Helpers;
using BauFahrplanMonitor.Core.Importer.Dto.Shared;
using BauFahrplanMonitor.Core.Importer.Helper;
using BauFahrplanMonitor.Core.Interfaces;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Models;
using Microsoft.EntityFrameworkCore;
using NLog;
using Npgsql;

namespace BauFahrplanMonitor.Core.Resolver;

/// <summary>
/// Zentrale Referenzauflösung für alle Importer.
/// </summary>
public class SharedReferenceResolver {
    private static readonly Logger Logger =
        LogManager.GetLogger("SharedReferenceResolver");

    // ----------------------------------------------
    // Dokument-Locks (Key: Vorgang + Dateiname)
    // ----------------------------------------------
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> VorgangLocks = new();

    private static readonly ConcurrentDictionary<(string Name, string Vorname, string Mail), SemaphoreSlim>
        SenderLocks = new();

    // =====================================================================
    // CACHES
    // =====================================================================

    private readonly ConcurrentDictionary<string, long> _regionCache = new();

    private readonly ConcurrentDictionary<(string Name, string Vorname, string Mail), long>
        _senderCache = new();

    private readonly ConcurrentDictionary<string, long> _kundeCache   = new();
    private readonly ConcurrentDictionary<string, long> _bstCache     = new();
    private readonly ConcurrentDictionary<long, long>   _streckeCache = new();
    private readonly ConcurrentDictionary<string, long> _bst2StrCache = new();

    private readonly ConcurrentDictionary<(long VorgangRef, string Bbmn), byte>
        _bbmnCache = new();

    private readonly ConcurrentDictionary<(long MasterFplo, int Fahrplanjahr), long>
        _vorgangCache = new();

    private readonly RegionCacheStats _regionStats = new();
    // =====================================================================
    // LOGGING HELPERS
    // =====================================================================

    private static void LogStart(string scope, string msg, params object?[] args) {
        if (!Logger.IsDebugEnabled) return;
        Logger.Debug($"→ {scope} {string.Format(msg, args)}");
    }

    private static void LogHit(string scope, string source, string msg, params object?[] args) {
        if (!Logger.IsDebugEnabled) return;
        Logger.Debug($"← {scope} ({source}) {string.Format(msg, args)}");
    }

    private static void LogCreated(string scope, string msg, params object?[] args) {
        // Created ist für dich besonders wichtig → Info
        Logger.Info($"＋ {scope} {string.Format(msg, args)}");
    }

    // =====================================================================
    // SENDER
    // =====================================================================
    public async Task<long> ResolveOrCreateSenderAsync(
        UjBauDbContext    db,
        SharedHeaderDto   header,
        CancellationToken token) {
        ArgumentNullException.ThrowIfNull(header);

        var key = (
            Name: Norm(header.SenderName),
            Vorname: Norm(header.SenderVorname),
            Mail: Norm(header.SenderMail)
        );

        LogStart("[Sender.ResolveOrCreate]", "name='{0}', vorname='{1}', mail='{2}', file='{3}'",
            key.Name, key.Vorname, key.Mail, header.FileName);

        // -------------------------------------------------
        // Fast Path: Cache vor Lock
        // -------------------------------------------------
        if (_senderCache.TryGetValue(key, out var cachedId))
            return cachedId;

        // -------------------------------------------------
        // Lock (prozesslokal)
        // -------------------------------------------------
        var sem = GetSenderLock(key);
        await sem.WaitAsync(token);

        try {
            if (_senderCache.TryGetValue(key, out cachedId))
                return cachedId;

            // -------------------------------------------------
            // DB Lookup (normalisiert)
            // -------------------------------------------------
            var sender = await db.UjbauSender.FirstOrDefaultAsync(
                s =>
                    (s.Name    ?? "").Trim().ToLower() == key.Name    &&
                    (s.Vorname ?? "").Trim().ToLower() == key.Vorname &&
                    (s.Email   ?? "").Trim().ToLower() == key.Mail,
                token);

            var isNew     = false;
            var isChanged = false;

            if (sender == null) {
                // CREATE (null-safe)
                sender = new UjbauSender {
                    Name      = (header.SenderName).Trim(),
                    Vorname   = (header.SenderVorname).Trim(),
                    Email     = (header.SenderMail).Trim(),
                    Abteilung = header.SenderAbteilung,
                    Telefon   = header.SenderTelefon,
                    Strasse   = header.SenderAdresse,
                    Stadt     = header.SenderStadt,
                    Plz       = header.SenderPlz // int? oder int → passt
                };

                db.UjbauSender.Add(sender);
                isNew = true;
            }
            else {
                var s = sender;
                isChanged |= UpdateIfDifferent(sender.Abteilung, header.SenderAbteilung, v => s.Abteilung = v);
                isChanged |= UpdateIfDifferent(sender.Telefon, header.SenderTelefon, v => s.Telefon       = v);
                isChanged |= UpdateIfDifferent(sender.Strasse, header.SenderAdresse, v => s.Strasse       = v);
                isChanged |= UpdateIfDifferent(sender.Stadt, header.SenderStadt, v => s.Stadt             = v);

                // UpdateIfDifferent ist nur für string → PLZ explizit
                if (sender.Plz != header.SenderPlz) {
                    sender.Plz = header.SenderPlz;
                    isChanged  = true;
                }

                if (isChanged) {
                    Logger.Info($"[Sender] aktualisiert: {sender.Name} {sender.Vorname} <{sender.Email}> id={sender.Id}");
                }
                else {
                    LogHit("[Sender.ResolveOrCreate]", "NoChange", "{0} {1} <{2}> → {3}",
                        sender.Name, sender.Vorname, sender.Email, sender.Id);
                }
            }

            // -------------------------------------------------
            // Persist + Unique-Catch
            // -------------------------------------------------
            try {
                if (isNew || isChanged) {
                    await db.SaveChangesAsync(token);
                    db.Entry(sender).State = EntityState.Unchanged;
                }
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex)) {
                // anderer Thread war schneller → Re-Read
                sender = await db.UjbauSender.SingleAsync(
                    s =>
                        (s.Name    ?? "").Trim().ToLower() == key.Name    &&
                        (s.Vorname ?? "").Trim().ToLower() == key.Vorname &&
                        (s.Email   ?? "").Trim().ToLower() == key.Mail,
                    token);
            }

            // -------------------------------------------------
            // Cache NACH erfolgreichem Persistieren
            // -------------------------------------------------
            _senderCache[key] = sender.Id;

            if (isNew) {
                LogCreated($"[Sender]", "angelegt: {0} {1} <{2}> → id={3}",
                    sender.Name, sender.Vorname, sender.Email, sender.Id);
            }

            return sender.Id;
        }
        finally {
            sem.Release();
        }
    }

    // =====================================================================
    // VORGANG
    // =====================================================================
    public async Task<long> ResolveOrCreateVorgangAsync(
        UjBauDbContext    db,
        SharedVorgangDto  dto,
        ImportMode        importMode,
        CancellationToken token) {
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.MasterFplo <= 0)
#pragma warning disable CA2208
            throw new ArgumentException("MasterFplo muss > 0 sein", nameof(dto.MasterFplo));
#pragma warning restore CA2208

        if (!dto.FahrplanJahr.HasValue)
#pragma warning disable CA2208
            throw new ArgumentException("Fahrplanjahr muss gesetzt sein", nameof(dto.FahrplanJahr));
#pragma warning restore CA2208

        var cacheKey = (dto.MasterFplo, dto.FahrplanJahr.Value);

        // -------------------------------------------------
        // Cache
        // -------------------------------------------------
        if (_vorgangCache.TryGetValue(cacheKey, out var cachedId))
            return cachedId;

        var sem = GetVorgangLock(dto.MasterFplo, dto.FahrplanJahr);
        await sem.WaitAsync(token);

        try {
            if (_vorgangCache.TryGetValue(cacheKey, out var cachedAfterLock))
                return cachedAfterLock;

            var vorgang = await db.UjbauVorgang
                .FirstOrDefaultAsync(v =>
                        v.VorgangNr    == dto.MasterFplo &&
                        v.Fahrplanjahr == dto.FahrplanJahr,
                    token);

            var isNew = false;

            if (vorgang == null) {
                vorgang = new UjbauVorgang {
                    VorgangNr    = dto.MasterFplo,
                    Fahrplanjahr = dto.FahrplanJahr,
                    Kategorie = string.IsNullOrWhiteSpace(dto.Kategorie)
                        ? "A"
                        : dto.Kategorie
                };

                db.UjbauVorgang.Add(vorgang);
                isNew = true;
            }

            if (dto is IExtendedVorgangDto extended) {
                if (isNew || importMode != ImportMode.UeB) {
                    extended.ApplyTo(vorgang);
                }
                else {
                    extended.ApplyIfEmptyTo(vorgang);
                }
            }

            try {
                await db.SaveChangesAsync(token);
                db.Entry(vorgang).State = EntityState.Unchanged;
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex)) {
                vorgang = await db.UjbauVorgang
                    .SingleAsync(v =>
                            v.VorgangNr    == dto.MasterFplo &&
                            v.Fahrplanjahr == dto.FahrplanJahr,
                        token);
            }

            _vorgangCache[cacheKey] = vorgang.Id;

            if (isNew)
                LogCreated("[Vorgang]", "angelegt: masterFplo={0}, fahrplanJahr={1} → id={2}",
                    dto.MasterFplo, dto.FahrplanJahr, vorgang.Id);
            else
                LogHit("[Vorgang.ResolveOrCreate]", "Resolved", "{0}/{1} → {2}",
                    dto.MasterFplo, dto.FahrplanJahr, vorgang.Id);

            return vorgang.Id;
        }
        finally {
            sem.Release();
        }
    }

    // =====================================================================
    // KUNDE
    // =====================================================================
    public async Task<long> ResolveOrCreateKundeAsync(
        UjBauDbContext    db,
        string?           kundeCode,
        CancellationToken token) {
        var key = string.IsNullOrWhiteSpace(kundeCode)
            ? "UNKNOWN"
            : kundeCode.Trim();

        LogStart("[Kunde.ResolveOrCreate]", "kundeCode='{0}' key='{1}'", kundeCode, key);

        // -------------------------
        // Cache
        // -------------------------
        if (_kundeCache.TryGetValue(key, out var cached)) {
            LogHit("[Kunde.ResolveOrCreate]", "Cache", "{0} → {1}", key, cached);
            return cached;
        }

        // -------------------------
        // Database lookup
        // -------------------------
        var kunde = await db.BasisKunde
            .FirstOrDefaultAsync(k => k.Kdnnr == key, token);

        if (kunde != null) {
            _kundeCache[key] = kunde.Id;
            LogHit("[Kunde.ResolveOrCreate]", "Database", "{0} → {1}", key, kunde.Id);
            return kunde.Id;
        }

        // -------------------------
        // Create (minimal!)
        // -------------------------
        kunde = new BasisKunde {
            Kdnnr = key
        };

        db.BasisKunde.Add(kunde);

        try {
            await db.SaveChangesAsync(token);

            _kundeCache[key] = kunde.Id;

            LogCreated("[Kunde]", "angelegt: '{0}' → id={1}", key, kunde.Id);

            return kunde.Id;
        }
        catch (DbUpdateException ex) {
            // Race condition: anderer Thread war schneller
            Logger.Warn(ex, "[Kunde] INSERT Race: '{0}' → Re-Read", key);

            var id = await db.BasisKunde
                .Where(k => k.Kdnnr == key)
                .Select(k => k.Id)
                .FirstOrDefaultAsync(token);

            _kundeCache[key] = id;

            LogHit("[Kunde.ResolveOrCreate]", "RaceWinner", "{0} → {1}", key, id);

            return id;
        }
    }

    // =====================================================================
    // REGION
    // =====================================================================
    public async Task<long> ResolveRegionAsync(
        UjBauDbContext    db,
        string?           raw,
        CancellationToken token) {
        var key = NormalizeRegionKey(raw);

        if (string.IsNullOrEmpty(key)) {
            Interlocked.Increment(ref _regionStats.CacheMisses);
            return 0;
        }

        // 1️⃣ Cache (Normalfall!)
        if (_regionCache.TryGetValue(key, out var cached)) {
            Interlocked.Increment(ref _regionStats.CacheHits);
            return cached;
        }

        // 2️⃣ Fallback: NUR rohe Vergleiche (EF-kompatibel)
        var rawLower = raw!.Trim().ToLowerInvariant();

        var region = await db.BasisRegion
            .AsNoTracking()
            .Where(r =>
                (r.Bezeichner != null && r.Bezeichner.ToLower() == rawLower) ||
                (r.Langname   != null && r.Langname.ToLower()   == rawLower) ||
                (r.Kbez       != null && r.Kbez.ToLower()       == rawLower)
            )
            .Select(r => r.Id)
            .FirstOrDefaultAsync(token);

        if (region > 0) {
            _regionCache[key] = region; // 🔑 normierter Key
            Interlocked.Increment(ref _regionStats.DbHits);
            return region;
        }

        _regionCache[key] = 0;
        Interlocked.Increment(ref _regionStats.CacheMisses);
        return 0;
    }
    
    // =====================================================================
    // BETRIEBSSTELLE
    // =====================================================================
    public async Task<long> ResolveOrCreateBetriebsstelleAsync(
        UjBauDbContext    db,
        string?           raw,
        CancellationToken token) {
        var key = Ds100Normalizer.Clean(raw);

        LogStart("[Bst.ResolveOrCreate]", "raw='{0}' key='{1}'", raw, key);

        if (string.IsNullOrWhiteSpace(key)) {
            Logger.Error($"Kein Key für {raw} gefunden");
            return -1;
        }

        // Cache
        if (_bstCache.TryGetValue(key, out var cached) && cached > 0) {
            LogHit("[Bst.ResolveOrCreate]", "Cache", "{0} → {1}", key, cached);
            return cached;
        }

        // DB Lookup
        var bst = await db.BasisBetriebsstelle
            .FirstOrDefaultAsync(b => b.Rl100 != null && b.Rl100.ToUpper() == key, token);

        if (bst != null) {
            _bstCache[key] = bst.Id;
            LogHit("[Bst.ResolveOrCreate]", "Database", "{0} → {1}", key, bst.Id);
            return bst.Id;
        }

        // CREATE Dummy
        bst = new BasisBetriebsstelle {
            Rl100             = key,
            NetzbezirkRef     = 0,
            TypRef            = 0,
            RegionRef         = 0,
            Zustand           = "in Betrieb",
            IstBasisDatensatz = false
        };

        db.BasisBetriebsstelle.Add(bst);
        await db.SaveChangesAsync(token);

        _bstCache[key] = bst.Id;

        Logger.Warn("[Bst] Betriebsstelle angelegt: RL100='{0}', id={1}", key, bst.Id);

        return bst.Id;
    }

    public async Task<long> ResolveOrCreateBetriebsstelleCachedAsync(
        UjBauDbContext    db,
        string?           raw,
        CancellationToken token) {

        var key = Ds100Normalizer.Clean(raw);
        if (string.IsNullOrWhiteSpace(key))
            return -1;

        if (_bstCache.TryGetValue(key, out var cached))
            return cached;

        var id = await ResolveOrCreateBetriebsstelleAsync(db, key, token);

        if (id > 0)
            _bstCache.TryAdd(key, id);

        return id;
    }

    // =====================================================================
    // STRECKE
    // =====================================================================
    public async Task<long> ResolveStreckeAsync(UjBauDbContext db, long? vzgNr, CancellationToken token = default) {
        LogStart("[Strecke.Resolve]", "vzgNr={0}", vzgNr);

        if (vzgNr is null or <= 0) {
            LogHit("[Strecke.Resolve]", "Invalid", "vzgNr={0} → 0", vzgNr);
            return 0;
        }

        if (_streckeCache.TryGetValue(vzgNr.Value, out var cached)) {
            LogHit("[Strecke.Resolve]", "Cache", "{0} → {1}", vzgNr.Value, cached);
            return cached;
        }

        var strecke = await db.BasisStrecke
            .FirstOrDefaultAsync(s => s.VzgNr == vzgNr, cancellationToken: token);

        if (strecke != null) {
            _streckeCache[vzgNr.Value] = strecke.Id;
            LogHit("[Strecke.Resolve]", "Database", "{0} → {1}", vzgNr.Value, strecke.Id);
            return strecke.Id;
        }

        _streckeCache[vzgNr.Value] = 0;
        LogHit("[Strecke.Resolve]", "Miss", "{0} → 0", vzgNr.Value);
        return 0;
    }

    // =====================================================================
    // BETRIEBSSTELLE ↔ STRECKE
    // =====================================================================
    public async Task<long> ResolveBst2StrAsync(
        UjBauDbContext    db,
        long              bstRef,
        long              strRef,
        string?           kmL   = null,
        CancellationToken token = default) {
        LogStart("[Bst2Str.Resolve]", "bstRef={0}, vzgNr={1}, kmL='{2}'", bstRef, strRef, kmL);

        var key = $"{bstRef}|{strRef}";

        if (_bst2StrCache.TryGetValue(key, out var cached)) {
            LogHit("[Bst2Str.Resolve]", "Cache", "{0} → {1}", key, cached);
            return cached;
        }

        var existing = await db.BasisBetriebsstelle2strecke
            .FirstOrDefaultAsync(x => x.BstRef == bstRef && x.StreckeRef == strRef, token);

        if (existing != null) {
            _bst2StrCache[key] = existing.Id;
            LogHit("[Bst2Str.Resolve]", "Database", "{0} → {1}", key, existing.Id);
            return existing.Id;
        }

        var neu = new BasisBetriebsstelle2strecke {
            BstRef            = bstRef,
            StreckeRef        = strRef,
            KmL               = string.IsNullOrWhiteSpace(kmL) ? null : kmL,
            IstBasisDatensatz = false,
        };

        db.BasisBetriebsstelle2strecke.Add(neu);

        try {
            await db.SaveChangesAsync(token);
            _bst2StrCache[key] = neu.Id;

            LogCreated("[Bst2Str]", "angelegt: {0} → id={1}", key, neu.Id);
            LogHit("[Bst2Str.Resolve]", "Created", "{0} → {1}", key, neu.Id);

            return neu.Id;
        }
        catch (DbUpdateException ex) {
            Logger.Warn(ex, "[Bst2Str] INSERT Race → Re-Read: {0}", key);

            var winner = await db.BasisBetriebsstelle2strecke
                .FirstOrDefaultAsync(x => x.BstRef == bstRef && x.StreckeRef == strRef, token);

            if (winner != null) {
                _bst2StrCache[key] = winner.Id;

                LogHit("[Bst2Str.Resolve]", "RaceWinner", "{0} → {1}", key, winner.Id);

                return winner.Id;
            }

            // ❗ DAS ist wichtig:
            // Kein Datensatz → kein Race → echter Fehler
            Logger.Error(ex, "[Bst2Str.Resolve] FK-Race ohne Gewinner: bstRef={0}, strRef={1}", bstRef, strRef);

            throw;
        }
    }

    // =====================================================================
    // HELPER
    // =====================================================================
    private static bool UpdateIfDifferent(
        string?        current,
        string?        incoming,
        Action<string> setter) {
        var newValue = incoming ?? string.Empty;
        var oldValue = current  ?? string.Empty;

        if (oldValue == newValue)
            return false;

        setter(newValue);
        return true;
    }

    private static string Norm(string? value) =>
        (value ?? string.Empty)
        .Trim()
        .ToLowerInvariant();

    private static SemaphoreSlim GetVorgangLock(long masterFplo, int? fahrplanjahr) {
        var key = $"VORGANG:{masterFplo}:{fahrplanjahr}";
        return VorgangLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
    }

    private static SemaphoreSlim GetSenderLock(
        (string Name, string Vorname, string Mail) key) {
        return SenderLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

    public bool TryRegisterBbmn(long vorgangRef, string bbmn)
        => _bbmnCache.TryAdd((vorgangRef, bbmn), 0);

    private static string NormalizeRegionKey(string? raw) {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        Logger.Info($"[NormalizeRegionKey] => Eingang: {raw}");
        
        var s = raw.Trim().ToLowerInvariant();

        // 🔑 RB / Rb / rb entfernen
        if (s.StartsWith("rb "))
            s = s[3..];

        /*s = s
            .Replace("ü", "ue")
            .Replace("ö", "oe")
            .Replace("ä", "ae")
            .Replace("ß", "ss");
        */
        s = Regex.Replace(s, @"\s+", " ");

        Logger.Info($"[NormalizeRegionKey] => Ausgang: {s}");
        
        return s;
    }

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

        Logger.Info($"[Region.Cache] Warm-Up abgeschlossen: {RegionCacheSize} Einträge");
    }

    public int              RegionCacheSize  => _regionCache.Count;
    public RegionCacheStats GetRegionStats() => _regionStats;
}