using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using BauFahrplanMonitor.Core.Interfaces;
using BauFahrplanMonitor.Helpers;

namespace BauFahrplanMonitor.Services;

public sealed class FileDialogService : IFileDialogService {

    public async Task<string?> OpenFileAsync(ImporterTyp importerTyp) {

        var lifetime =
            Application.Current?.ApplicationLifetime
                as IClassicDesktopStyleApplicationLifetime;

        var window = lifetime?.MainWindow;
        if (window is null)
            return null;

        var options = new FilePickerOpenOptions {
            Title          = "Importdatei auswählen",
            AllowMultiple  = false,
            FileTypeFilter = BuildFilters(importerTyp)
        };

        var result = await window.StorageProvider.OpenFilePickerAsync(options);
        var file   = result.Count > 0 ? result[0] : null;

        return file?.Path.LocalPath;
    }

    private static IReadOnlyList<FilePickerFileType> BuildFilters(
        ImporterTyp importerTyp) {

        return importerTyp switch {

            ImporterTyp.BBPNeo => new[] {
                new FilePickerFileType("BBPNeo XML") {
                    Patterns = ["BBP*.xml"]
                }
            },

            ImporterTyp.OsbBob => new[] {
                new FilePickerFileType("OsbBob XML") {
                    Patterns = ["*.csv"]
                }
            },

            _ => new[] {
                FilePickerFileTypes.All
            }
        };
    }
}