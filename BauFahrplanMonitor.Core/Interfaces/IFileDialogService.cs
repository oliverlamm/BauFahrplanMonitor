using BauFahrplanMonitor.Core.Helpers;

namespace BauFahrplanMonitor.Core.Interfaces;

public interface IFileDialogService {
    Task<string?> OpenFileAsync(ImporterTyp importerTyp);
}
