using BauFahrplanMonitor.Importer.Helper;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BauFahrplanMonitor.ViewModels;

public abstract class ImportStatisticsViewModel : ObservableObject {
    /// <summary>
    /// Wird beim Start eines Imports zurückgesetzt
    /// </summary>
    public abstract void Reset();

    /// <summary>
    /// Wird bei jedem Progress-Update aufgerufen
    /// </summary>
    public abstract void Update(ImportProgressInfo info);
}