using System.Threading.Tasks;
using BauFahrplanMonitor.Helpers;

namespace BauFahrplanMonitor.Interfaces;

public interface IFileDialogService {
    Task<string?> OpenFileAsync(ImporterTyp importerTyp);
}
