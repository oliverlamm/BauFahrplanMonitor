using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Helpers;
using BauFahrplanMonitor.Importer.Dto.BbpNeo;
using BauFahrplanMonitor.Importer.Helper;
using BauFahrplanMonitor.Importer.Interface;
using BauFahrplanMonitor.Importer.Mapper;
using BauFahrplanMonitor.Interfaces;
using BauFahrplanMonitor.Services;

namespace BauFahrplanMonitor.Importer;

public sealed class BbpNeoImporter : IFileImporter {
    private readonly ConfigService             _config;
    private readonly IBbpNeoXmlStreamingLoader _streamingLoader;
    private readonly IBbpNeoUpsertService      _upsert;

    public BbpNeoImporter(
        ConfigService             config,
        IBbpNeoXmlStreamingLoader streamingLoader,
        IBbpNeoUpsertService      upsert) {
        _config          = config;
        _streamingLoader = streamingLoader;
        _upsert          = upsert;
    }

    // -------------------------------------------------
    // Worker-Berechnung
    // -------------------------------------------------
    private int CalculateWorkerCount() {
        if (_config.Effective.Allgemein.Debugging)
            return 1;

        var requested = Math.Max(1, _config.Effective.Allgemein.ImportThreads);
        var cpu       = Environment.ProcessorCount;

        return Math.Clamp(requested, 1, Math.Max(1, cpu - 2));
    }

    // -------------------------------------------------
    // Import
    // -------------------------------------------------
    public async Task ImportAsync(
        UjBauDbContext                db,
        ImportFileItem                item,
        IProgress<ImportProgressInfo> progress,
        CancellationToken             token) {
        var filePath    = item.FilePath;
        var workerCount = CalculateWorkerCount();

        var processed = 0;

        // -------------------------------------------------
        // Queue (NICHT per using!)
        // -------------------------------------------------
        var queue = new BlockingCollection<BbpNeoMassnahmeRaw>(workerCount * 2);

        // -------------------------------------------------
        // Worker
        // -------------------------------------------------
        var workers = new Task[workerCount];

        for (var i = 0; i < workerCount; i++) {
            var workerIndex = i + 1;

            workers[i] = Task.Run(async () => {
                foreach (var raw in queue.GetConsumingEnumerable(token)) {
                    token.ThrowIfCancellationRequested();

                    // Normalisierung
                    var result   = BbpNeoNormalizer.Normalize(raw);
                    var domain   = result.Value;
                    var warnings = result.Warnings;

                    // Persistenz
                    await _upsert.UpsertMassnahmeWithChildrenAsync(
                        db,
                        domain,
                        warnings,
                        token);

                    var done = Interlocked.Increment(ref processed);

                    progress.Report(new ImportProgressInfo {
                        TotalItems     = -1,
                        ProcessedItems = done,

                        WorkerIndex = workerIndex,
                        WorkerDone  = done,
                        WorkerTotal = -1
                    });
                }
            });
        }

        // -------------------------------------------------
        // Producer (Streaming XML)
        // -------------------------------------------------
        try {
            await _streamingLoader.StreamAsync(
                filePath,
                (massnahme, ct) => {
                    ct.ThrowIfCancellationRequested();
                    queue.Add(massnahme, ct);
                    return Task.CompletedTask;
                },
                token);
        }
        finally {
            queue.CompleteAdding();
        }

        await Task.WhenAll(workers);

        // 🔑 JETZT ist es sicher
        queue.Dispose();
    }
}