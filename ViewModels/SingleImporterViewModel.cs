using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using BauFahrplanMonitor.Data;
using BauFahrplanMonitor.Helpers;
using BauFahrplanMonitor.Importer.Helper;
using BauFahrplanMonitor.Importer.Interface;
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
    private readonly Window                            _owner;

    private CancellationTokenSource? _cts;

    // =====================================================
    // Header
    // =====================================================
    public string Title { get; }

    private ImporterStatus _status = ImporterStatus.Bereit;

    public ImporterStatus Status {
        get => _status;
        private set {
            if (SetProperty(ref _status, value)) {
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(StatusBrush));
            }
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
    private int _totalItems;

    public int TotalItems {
        get => _totalItems;
        private set {
            SetProperty(ref _totalItems, value);
            OnPropertyChanged(nameof(OverallProgress));
            OnPropertyChanged(nameof(OverallProgressText));
        }
    }

    private int _processedItems;

    public int ProcessedItems {
        get => _processedItems;
        private set {
            SetProperty(ref _processedItems, value);
            OnPropertyChanged(nameof(OverallProgress));
            OnPropertyChanged(nameof(OverallProgressText));
        }
    }

    public double OverallProgress =>
        TotalItems == 0 ? 0 : (double)ProcessedItems / TotalItems;

    public string OverallProgressText =>
        $"{ProcessedItems:N0} / {TotalItems:N0}";

    // =====================================================
    // Threads
    // =====================================================
    public ObservableCollection<ImportWorkerViewModel> Threads { get; }
        = new();

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

    public bool ShowFilters => _importerTyp == ImporterTyp.ZvFExport;

    // =====================================================
    // ctor
    // =====================================================
    public SingleFileImporterViewModel(
        Window                            owner,
        ConfigService                     config,
        StatusMessageService              statusMessages,
        IFileImporterFactory              importerFactory,
        IDbContextFactory<UjBauDbContext> dbFactory,
        SharedReferenceResolver           resolver,
        ImporterTyp                       importerTyp,
        string                            title) {
        _config          = config;
        _statusMessages  = statusMessages;
        _importerFactory = importerFactory;
        _dbFactory       = dbFactory;
        _resolver        = resolver;
        _importerTyp     = importerTyp;
        _owner           = owner;

        Title = title;

        CreateWorkers();
    }

    private void CreateWorkers() {
        Threads.Clear();

        var count = _config.Effective.Allgemein.ImportThreads;
        if (_config.Effective.Allgemein.Debugging)
            count = 1;

        for (var i = 1; i <= count; i++)
            Threads.Add(new ImportWorkerViewModel(i));
    }

    // =====================================================
    // Commands
    // =====================================================
    [RelayCommand]
    private async Task SelectFile() {
        var dlg = new OpenFileDialog {
            Title         = "Importdatei auswählen",
            AllowMultiple = false
        };

        var result = await dlg.ShowAsync(_owner);
        if (result?.Length > 0)
            ImportFile = result[0];
    }

    [RelayCommand]
    private async Task Start() {
        if (!CanStart)
            return;

        IsRunning = true;
        Status    = ImporterStatus.Importieren;

        _cts = new CancellationTokenSource();

        TotalItems     = 0;
        ProcessedItems = 0;

        try {
            await using var db = await _dbFactory.CreateDbContextAsync(_cts.Token);
            await _resolver.WarmUpRegionCacheAsync(db, _cts.Token);

            var importer = _importerFactory.GetImporter(_importerTyp);

            var item = new ImportFileItem(
                ImportFile!,
                DateTime.UtcNow,
                ImportMode.None);

            var progress = new Progress<ImportProgressInfo>(info => {
                Dispatcher.UIThread.Post(() => {
                    TotalItems     = info.TotalItems;
                    ProcessedItems = info.ProcessedItems;

                    var worker = Threads[info.WorkerIndex - 1];
                    worker.Status = ImportThreadStatus.Importieren;
                    worker.Total  = info.WorkerTotal;
                    worker.Done   = info.WorkerDone;

                    worker.UpdateStat("Anzahl Maßnahmen",  info.MeasuresDone);
                    worker.UpdateStat("Anzahl Regelungen", info.Regelungen);
                    worker.UpdateStat("Anzahl BvE",        info.BvE);
                    worker.UpdateStat("Anzahl APS",        info.APS);
                    worker.UpdateStat("Anzahl IAV",        info.IAV);
                });
            });

            await importer.ImportAsync(db, item, progress, _cts.Token);

            Status = ImporterStatus.Abgeschlossen;
            _statusMessages.Success("Import abgeschlossen");
        }
        catch (OperationCanceledException) {
            Status = ImporterStatus.Abbruch;
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
}