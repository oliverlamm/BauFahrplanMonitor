using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Helpers;
using BauFahrplanMonitor.Importer.Helper;
using BauFahrplanMonitor.Interfaces;
using BauFahrplanMonitor.Resolver;
using BauFahrplanMonitor.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace BauFahrplanMonitor.ViewModels;

public sealed partial class SingleFileImporterViewModel : ObservableObject {
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly ConfigService                     _config;
    private readonly StatusMessageService              _statusMessages;
    private readonly IFileImporterFactory              _importerFactory;
    private readonly IDbContextFactory<UjBauDbContext> _dbFactory;
    private readonly SharedReferenceResolver           _resolver;
    private readonly ImporterTyp                       _importerTyp;
    private readonly IFileDialogService                _fileDialog;

    private CancellationTokenSource? _cts;

    // =====================================================
    // Header
    // =====================================================
    public  string                    Title      { get; }
    public  ImportStatisticsViewModel Statistics { get; }
    private ImporterStatus            _status = ImporterStatus.Bereit;

    public ImporterStatus Status {
        get => _status;
        private set {
            if (!SetProperty(ref _status, value)) return;
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusBrush));
        }
    }

    public string StatusText =>
        Status switch {
            ImporterStatus.Bereit        => "Bereit",
            ImporterStatus.Importieren   => "Importiere…",
            ImporterStatus.Abgeschlossen => "Abgeschlossen",
            ImporterStatus.Abbruch       => "Abbruch",
            ImporterStatus.Fehler        => "Fehler",
            _                            => "Unbekannt"
        };

    public IBrush StatusBrush =>
        Status switch {
            ImporterStatus.Bereit        => Brushes.Gray,
            ImporterStatus.Importieren   => Brushes.DodgerBlue,
            ImporterStatus.Abgeschlossen => Brushes.ForestGreen,
            ImporterStatus.Abbruch       => Brushes.OrangeRed,
            ImporterStatus.Fehler        => Brushes.IndianRed,
            _                            => Brushes.Gray
        };

    // =====================================================
    // Datei
    // =====================================================
    private string? _importFile;

    public string? ImportFile {
        get => _importFile;
        private set {
            SetProperty(ref _importFile, value);
            OnPropertyChanged(nameof(CanStart));
        }
    }

    // =====================================================
    // Progress (Overall = Maßnahmen)
    // =====================================================

    [ObservableProperty] private int processedItems;

    [ObservableProperty] private int totalItems;

    /// <summary>
    /// Fortschritt 0.0 – 1.0 (für ProgressBar)
    /// </summary>
    public double OverallProgress =>
        TotalItems > 0
            ? (double)ProcessedItems / TotalItems
            : 0;

    /// <summary>
    /// Fortschritt in Prozent (0–100)
    /// </summary>
    public int OverallProgressPercent =>
        TotalItems > 0
            ? (int)Math.Round((double)ProcessedItems / TotalItems * 100)
            : 0;

    /// <summary>
    /// Textdarstellung, z. B. "36 %"
    /// </summary>
    public string ProgressPercentText =>
        $"{OverallProgressPercent} %";

    /// <summary>
    /// Textdarstellung, z. B. "50 / 140"
    /// </summary>
    public string OverallProgressText =>
        $"{ProcessedItems:N0} / {TotalItems:N0}";

    // =====================================================
    // Property-Change Hooks (CommunityToolkit)
    // =====================================================

    partial void OnProcessedItemsChanged(int value) {
        OnPropertyChanged(nameof(OverallProgress));
        OnPropertyChanged(nameof(OverallProgressPercent));
        OnPropertyChanged(nameof(ProgressPercentText));
        OnPropertyChanged(nameof(OverallProgressText));
    }

    partial void OnTotalItemsChanged(int value) {
        OnPropertyChanged(nameof(OverallProgress));
        OnPropertyChanged(nameof(OverallProgressPercent));
        OnPropertyChanged(nameof(ProgressPercentText));
        OnPropertyChanged(nameof(OverallProgressText));
    }

    // =====================================================
    // Threads
    // =====================================================
    public ObservableCollection<ImportWorkerViewModel> Threads { get; }
        = [];

    // =====================================================
    // UI State
    // =====================================================
    private bool _isRunning;

    public bool IsRunning {
        get => _isRunning;
        private set {
            SetProperty(ref _isRunning, value);
            OnPropertyChanged(nameof(CanStart));
            OnPropertyChanged(nameof(CanStop));
        }
    }

    public bool CanStart =>
        !IsRunning                             &&
        !string.IsNullOrWhiteSpace(ImportFile) &&
        File.Exists(ImportFile);

    public bool CanStop => IsRunning;

    // =====================================================
    // ctor
    // =====================================================
    public SingleFileImporterViewModel(
        ConfigService                     config,
        StatusMessageService              statusMessages,
        IFileImporterFactory              importerFactory,
        IDbContextFactory<UjBauDbContext> dbFactory,
        SharedReferenceResolver           resolver,
        IFileDialogService                fileDialog,
        ImporterTyp                       importerTyp,
        string                            title) {
        _config          = config;
        _statusMessages  = statusMessages;
        _importerFactory = importerFactory;
        _dbFactory       = dbFactory;
        _resolver        = resolver;
        _importerTyp     = importerTyp;
        _fileDialog      = fileDialog;

        Title = title;
        Statistics = importerTyp switch {
            ImporterTyp.BBPNeo => new BbpNeoStatisticsViewModel(),
            _                  => throw new NotSupportedException(importerTyp.ToString())
        };
        CreateWorkers();
    }

    private void CreateWorkers() {
        Threads.Clear();

        var worker = new ImportWorkerViewModel(1) {
            Status = ImportThreadStatus.Bereit
        };

        Threads.Add(worker);
    }


    // =====================================================
    // Commands
    // =====================================================
    [RelayCommand]
    private async Task SelectFile() {

        var file = await _fileDialog.OpenFileAsync(_importerTyp);
        if (file is null)
            return;

        ImportFile = file;

        await PrepareImportAsync(file);
    }

    [RelayCommand]
    private async Task Start() {
        if (!CanStart)
            return;

        IsRunning = true;
        Status    = ImporterStatus.Importieren;
        var worker = Threads[0];
        worker.ResetTiming(); // neu

        Statistics.Reset();

        _cts = new CancellationTokenSource();

        ProcessedItems = 0;

        try {
            await using var db =
                await _dbFactory.CreateDbContextAsync(_cts.Token);

            // Cache-Warmup wie im MultiFileImport
            await _resolver.WarmUpRegionCacheAsync(db, _cts.Token);

            var importer = _importerFactory.GetImporter(_importerTyp);

            var item = new ImportFileItem(
                ImportFile!,
                DateTime.UtcNow,
                ImportMode.None);

            var progress = new Progress<ImportProgressInfo>(info => {
                Dispatcher.UIThread.Post(() => {
                    // OVERALL
                    ProcessedItems = info.ProcessedItems;
                    if (info.TotalItems > 0)
                        TotalItems = info.TotalItems;

                    // PIPELINE-WORKER (immer einer!)
                    var worker = Threads[0];

                    if (worker is {
                            IsFinal: false,
                            Status: ImportThreadStatus.Bereit
                        })
                        worker.Status = ImportThreadStatus.Importieren;

                    worker.Done  = info.ProcessedItems;
                    worker.Total = info.TotalItems;

                    // ==============================
                    // STATISTIK (Importer-Fakten)
                    // ==============================
                    worker.UpdateStat("Anzahl Maßnahmen", info.MeasuresDone);
                    worker.UpdateStat("Anzahl Regelungen", info.Regelungen);
                    worker.UpdateStat("Anzahl BvE", info.BvE);
                    worker.UpdateStat("Anzahl APS", info.APS);
                    worker.UpdateStat("Anzahl IAV", info.IAV);

                    // Pipeline-Metriken (Weg B)
                    worker.UpdateStat("Queue-Größe", info.QueueDepth);
                    worker.UpdateStat("Consumer aktiv", info.ActiveConsumers);
                    worker.UpdateStat("Consumer gesamt", info.TotalConsumers);

                    // ==============================
                    // ETA (UI-abgeleitet)
                    // ==============================
                    if (worker.Done < 3 || worker.Total <= 0)
                        return;
                    var elapsed = DateTime.UtcNow - worker.StartTime!.Value;

                    if (!(elapsed.TotalSeconds >= 1))
                        return;
                    var rate = worker.Done / elapsed.TotalSeconds;

                    if (!(rate > 0))
                        return;
                    var remaining = worker.Total - worker.Done;
                    var eta       = TimeSpan.FromSeconds(remaining / rate);

                    worker.UpdateStat("ETA", eta.ToString(@"hh\:mm\:ss"));
                    worker.UpdateStat("Durchsatz", rate.ToString("N1") + " /s");
                });
            });

            await importer.ImportAsync(db, item, progress, _cts.Token);

            Status = ImporterStatus.Abgeschlossen;
            _statusMessages.Success("Import abgeschlossen");
        }
        catch (OperationCanceledException) {
            Status = ImporterStatus.Abbruch;

            Dispatcher.UIThread.Post(() => { Threads[0].Status = ImportThreadStatus.Abbruch; });
        }

        catch (Exception ex) {
            Logger.Error(ex);
            Status = ImporterStatus.Fehler;
            _statusMessages.Error("Fehler beim Import");
        }
        finally {
            IsRunning = false;
            _cts      = null;
        }
    }

    [RelayCommand]
    private void Stop() {
        _cts?.Cancel();
    }

    private async Task PrepareImportAsync(string file) {

        // Reset
        TotalItems     = 0;
        ProcessedItems = 0;

        if (_importerTyp != ImporterTyp.BBPNeo)
            return;

        try {
            var importer = _importerFactory.GetImporter(_importerTyp);

            if (importer is not IBbpNeoImporter bbpNeo)
                return;

            var header = await bbpNeo.ReadHeaderAsync(file);

            await Dispatcher.UIThread.InvokeAsync(() => {
                TotalItems     = header.AnzMas;
                ProcessedItems = 0;

                foreach (var worker in Threads) {
                    worker.Total  = header.AnzMas;
                    worker.Done   = 0;
                    worker.Status = ImportThreadStatus.Bereit;
                }

                Statistics.Reset();
            });
        }
        catch (Exception ex) {
            Logger.Error(ex);
            _statusMessages.Error("Header der Datei konnte nicht gelesen werden.");
            ImportFile = null;
        }
    }
}