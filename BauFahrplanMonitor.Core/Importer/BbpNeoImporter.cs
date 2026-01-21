using System.Globalization;
using System.Threading.Channels;
using System.Xml;
using BauFahrplanMonitor.Core.Helpers;
using BauFahrplanMonitor.Core.Importer.Dto.BbpNeo;
using BauFahrplanMonitor.Core.Importer.Helper;
using BauFahrplanMonitor.Core.Importer.Mapper;
using BauFahrplanMonitor.Core.Interfaces;
using BauFahrplanMonitor.Core.Services;
using BauFahrplanMonitor.Data;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace BauFahrplanMonitor.Core.Importer;

public sealed class BbpNeoImporter(
    ConfigService                     config,
    IBbpNeoXmlStreamingLoader         streamingLoader,
    IBbpNeoUpserter                   upserter,
    IDbContextFactory<UjBauDbContext> dbFactory)
    : IBbpNeoImporter {

    private static readonly Logger Logger     = LogManager.GetCurrentClassLogger();
    private                 int    _measuresDone;
    private                 int    _regelungen;
    private                 int    _bve;
    private                 int    _aps;
    private                 int    _iav;
    private                 int    _activeConsumers;
    private                 int    _expectedItems;

    // -------------------------------------------------
    // Worker-Berechnung
    // -------------------------------------------------
    private int CalculateWorkerCount() {
        var maxThreads = 1;
        if (!config.Effective.Allgemein.Debugging) {

            var requested = Math.Max(1, config.Effective.Allgemein.ImportThreads);
            var cpu = Environment.ProcessorCount;

            maxThreads = Math.Clamp(requested, 1, Math.Max(1, cpu - 2));
        }

        Logger.Info($"Benutze {maxThreads} Consumers");
        return maxThreads;
    }

    // -------------------------------------------------
    // Import
    // -------------------------------------------------
    public async Task<ImportFileOutcome> ImportAsync(
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
        var header = await ReadHeaderAsync(filePath, token);
        _expectedItems = header.AnzMas;

        progress.Report(new ImportProgressInfo {
            TotalItems     = _expectedItems,
            ProcessedItems = 0,

            MeasuresDone = 0,
            Regelungen   = 0,
            BvE          = 0,
            APS          = 0,
            IAV          = 0,

            ActiveConsumers = 0,
            TotalConsumers  = workerCount,
            QueueDepth      = 0
        });
        
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

            workers[i] = Task.Run(async () => {
                await foreach (var raw in channel.Reader.ReadAllAsync(token)) {
                    token.ThrowIfCancellationRequested();

                    Interlocked.Increment(ref _activeConsumers);
                    try {
                        await using var db = await dbFactory.CreateDbContextAsync(token);

                        // Normalize
                        var result   = BbpNeoNormalizer.Normalize(raw);
                        var domain   = result.Value;
                        var warnings = result.Warnings;

                        // Upsert (ohne SaveChanges)
                        await upserter.UpsertMassnahmeWithChildrenAsync(
                            db,
                            domain,
                            warnings,
                            onRegelungUpserted: () => Interlocked.Increment(ref _regelungen),
                            onBveUpserted: () => Interlocked.Increment(ref _bve),
                            onApsUpserted: () => Interlocked.Increment(ref _aps),
                            onIavUpserted: () => Interlocked.Increment(ref _iav),
                            token);
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
            await streamingLoader.StreamAsync(
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
        return ImportFileOutcome.Success;
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