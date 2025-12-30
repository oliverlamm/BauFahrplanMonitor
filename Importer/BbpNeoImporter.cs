using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Helpers;
using BauFahrplanMonitor.Importer.Dto.BbpNeo;
using BauFahrplanMonitor.Importer.Helper;
using BauFahrplanMonitor.Importer.Mapper;
using BauFahrplanMonitor.Interfaces;
using BauFahrplanMonitor.Services;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace BauFahrplanMonitor.Importer;

public sealed class BbpNeoImporter : IBbpNeoImporter {
    private readonly ConfigService             _config;
    private readonly IBbpNeoXmlStreamingLoader _streamingLoader;
    private readonly IBbpNeoUpserter           _upserter;

    private static readonly Logger                            Logger = LogManager.GetCurrentClassLogger();
    private readonly        IDbContextFactory<UjBauDbContext> _dbFactory;
    private                 int                               _measuresDone;
    private                 int                               _regelungen;
    private                 int                               _bve;
    private                 int                               _aps;
    private                 int                               _iav;
    private                 int                               _activeConsumers;
    private                 int                               _expectedItems;
    private                 long                              _lastUiTick;
    private static readonly long                              _uiTickInterval = Stopwatch.Frequency / 5;
    private static readonly SemaphoreSlim                     _saveGate       = new(1, 1);

    public BbpNeoImporter(
        ConfigService                     config,
        IBbpNeoXmlStreamingLoader         streamingLoader,
        IBbpNeoUpserter                   upserter,
        IDbContextFactory<UjBauDbContext> dbFactory) {
        _config          = config;
        _streamingLoader = streamingLoader;
        _upserter        = upserter;
        _dbFactory       = dbFactory;
    }

    // -------------------------------------------------
    // Worker-Berechnung
    // -------------------------------------------------
    private int CalculateWorkerCount() {
        var maxThreads = 1;
        if (!_config.Effective.Allgemein.Debugging) {

            var requested = Math.Max(1, _config.Effective.Allgemein.ImportThreads);
            requested = requested >= 6 ? 6 : requested;
            var cpu = Environment.ProcessorCount;

            maxThreads = Math.Clamp(requested, 1, Math.Max(1, cpu - 2));
        }

        Logger.Info($"Benutze {maxThreads} Consumers");
        return maxThreads;
    }

    // -------------------------------------------------
    // Import
    // -------------------------------------------------
    public async Task ImportAsync(
        UjBauDbContext                dbs,
        ImportFileItem                item,
        IProgress<ImportProgressInfo> progress,
        CancellationToken             token) {
        var filePath    = item.FilePath;
        var workerCount = CalculateWorkerCount();

        // Reset Counters
        _measuresDone    = 0;
        _regelungen      = 0;
        _bve             = 0;
        _aps             = 0;
        _iav             = 0;
        _activeConsumers = 0;

        // Optional: expected items aus Header setzen (wenn du willst – UI egal)
        // var header = await ReadHeaderAsync(filePath, token);
        // _expectedItems = header.AnzMas;

        // -----------------------------
        // Channel (bounded backpressure)
        // -----------------------------
        var channel = Channel.CreateBounded<BbpNeoMassnahmeRaw>(
            new BoundedChannelOptions(capacity: Math.Max(4, workerCount * 2)) {
                SingleWriter = true,
                SingleReader = false,
                FullMode     = BoundedChannelFullMode.Wait
            });

        // -----------------------------
        // Consumers
        // -----------------------------
        var workers = new Task[workerCount];
        for (var i = 0; i < workerCount; i++) {
            var workerIndex = i + 1;

            workers[i] = Task.Run(async () => {
                await foreach (var raw in channel.Reader.ReadAllAsync(token)) {
                    token.ThrowIfCancellationRequested();

                    Interlocked.Increment(ref _activeConsumers);
                    try {
                        // 🔑 1 Maßnahme = 1 DbContext (kurzlebig)
                        await using var db = await _dbFactory.CreateDbContextAsync(token);

                        // EF “Import Mode”
                        db.ChangeTracker.AutoDetectChangesEnabled = false;
                        db.ChangeTracker.QueryTrackingBehavior    = QueryTrackingBehavior.NoTracking;

                        // Normalize
                        var result   = BbpNeoNormalizer.Normalize(raw);
                        var domain   = result.Value;
                        var warnings = result.Warnings;

                        // Upsert (ohne SaveChanges)
                        await _upserter.UpsertMassnahmeWithChildrenAsync(
                            db,
                            domain,
                            warnings,
                            onRegelungUpserted: () => Interlocked.Increment(ref _regelungen),
                            onBveUpserted: () => Interlocked.Increment(ref _bve),
                            onApsUpserted: () => Interlocked.Increment(ref _aps),
                            onIavUpserted: () => Interlocked.Increment(ref _iav),
                            token);

                        // SaveChanges (pro Maßnahme)
                        Logger.Info("DB SELECT 1 OK MasNr={0}", domain.MasId);
                        await db.Database.ExecuteSqlRawAsync("SELECT 1;", token);

                        await using var tx = await db.Database.BeginTransactionAsync(token);

                        // Timeouts gelten jetzt wirklich für das folgende SaveChanges
                        await db.Database.ExecuteSqlRawAsync("SET LOCAL lock_timeout = '5s';", token);
                        await db.Database.ExecuteSqlRawAsync("SET LOCAL statement_timeout = '120s';", token);

                        Logger.Info(">>> BEFORE SaveChanges MasNr={0}", domain.MasId);

                        using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                        cts.CancelAfter(TimeSpan.FromMinutes(2));
                        Logger.Info("ChangeTracker: Entries={0}", db.ChangeTracker.Entries().Count());
                        await db.SaveChangesAsync(cts.Token);

                        Logger.Info("<<< AFTER SaveChanges MasNr={0}", domain.MasId);

                        await tx.CommitAsync(token);

                        // (eigentlich nicht nötig, weil Context weg-disposed wird,
                        // aber schadet nicht, falls du später Context pooling aktivierst)
                        db.ChangeTracker.Clear();
                    }
                    finally {
                        Interlocked.Decrement(ref _activeConsumers);
                    }

                    var done = Interlocked.Increment(ref _measuresDone);

                    // Progress (UI kann später)
                    progress.Report(new ImportProgressInfo {
                        ProcessedItems = done,
                        TotalItems     = _expectedItems,

                        QueueDepth      = channel.Reader.Count,
                        ActiveConsumers = Volatile.Read(ref _activeConsumers),
                        TotalConsumers  = workerCount,

                        MeasuresDone = done,
                        Regelungen   = Volatile.Read(ref _regelungen),
                        BvE          = Volatile.Read(ref _bve),
                        APS          = Volatile.Read(ref _aps),
                        IAV          = Volatile.Read(ref _iav),
                    });
                }
            }, token);
        }

        // -----------------------------
        // Producer (Streaming XML)
        // -----------------------------
        try {
            await _streamingLoader.StreamAsync(
                filePath,
                async (massnahme, ct) => {
                    ct.ThrowIfCancellationRequested();
                    await channel.Writer.WriteAsync(massnahme, ct);
                },
                token);
        }
        finally {
            channel.Writer.TryComplete();
        }

        // Wait consumers
        await Task.WhenAll(workers);
    }

    // -------------------------------------------------
    // Header lesen
    // -------------------------------------------------
    public Task<BbpNeoHeaderInfo> ReadHeaderAsync(
        string            filePath,
        CancellationToken token = default) {
        return Task.Run(() => {
            var header = new BbpNeoHeaderInfo();

            Logger.Debug($"[BBPNeo.Header] Lese Header aus Datei: {filePath}");

            using var stream = File.OpenRead(filePath);
            using var reader = XmlReader.Create(stream, new XmlReaderSettings {
                IgnoreComments   = true,
                IgnoreWhitespace = true,
                DtdProcessing    = DtdProcessing.Ignore
            });

            reader.MoveToContent();

            if (!reader.ReadToFollowing("Header"))
                throw new InvalidOperationException(
                    "BBPNeo-Datei enthält kein <Header>-Element.");

            Logger.Debug("[BBPNeo.Header] <Header>-Element gefunden.");

            // Jetzt stehen wir auf <Header>, nächster Read geht INS Element
            reader.Read();

            while (!(reader is {
                       NodeType: XmlNodeType.EndElement,
                       LocalName: "Header"
                   })) {
                token.ThrowIfCancellationRequested();

                if (reader.NodeType != XmlNodeType.Element) {
                    reader.Read();
                    continue;
                }

                Logger.Debug($"[BBPNeo.Header] Element: Name='{reader.LocalName}'");

                switch (reader.LocalName) {
                    case "AnzMas":
                        header.AnzMas = reader.ReadElementContentAsInt();
                        Logger.Debug($"[BBPNeo.Header] AnzMas = {header.AnzMas}");
                        break;

                    case "Ersteller":
                        header.Ersteller = reader.ReadElementContentAsString();
                        break;

                    case "BBPVersion":
                        header.BBPVersion = reader.ReadElementContentAsString();
                        break;

                    case "BplBeginn":
                        header.BplBeginn = DateOnly.Parse(
                            reader.ReadElementContentAsString(),
                            CultureInfo.InvariantCulture);
                        break;

                    case "BplEnde":
                        header.BplEnde = DateOnly.Parse(
                            reader.ReadElementContentAsString(),
                            CultureInfo.InvariantCulture);
                        break;

                    default:
                        // 🔑 WICHTIG:
                        // leere ODER unbekannte Elemente sauber überspringen
                        if (reader.IsEmptyElement)
                            reader.Read();
                        else
                            reader.ReadElementContentAsString();
                        break;
                }
            }

            Logger.Debug(
                $"[BBPNeo.Header] Parsed Header: "    +
                $"AnzMas={header.AnzMas}, "           +
                $"Ersteller='{header.Ersteller}', "   +
                $"BBPVersion='{header.BBPVersion}', " +
                $"BplBeginn={header.BplBeginn}, "     +
                $"BplEnde={header.BplEnde}"
            );

            if (header.AnzMas <= 0)
                throw new InvalidOperationException(
                    "Header enthält keine gültige AnzMas.");

            _expectedItems = header.AnzMas;
            return header;

        }, token);
    }
}