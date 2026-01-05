using System;
using System.Threading;
using System.Threading.Tasks;
using BauFahrplanMonitor.Importer.Dto.BbpNeo;

namespace BauFahrplanMonitor.Interfaces;

/// <summary>
/// Streaming-Loader für BBPNeo-XML-Dateien.
///
/// Liest eine BBPNeo-Datei sequenziell (forward-only)
/// und liefert jede Maßnahme genau einmal an den Consumer.
/// </summary>
public interface IBbpNeoXmlStreamingLoader {
    /// <summary>
    /// Startet das Streaming einer BBPNeo-XML-Datei.
    /// </summary>
    /// <param name="filePath">
    /// Pfad zur BBPNeo-XML-Datei.
    /// </param>
    /// <param name="onMassnahme">
    /// Callback, das für jede geparste Maßnahme
    /// exakt einmal aufgerufen wird.
    /// </param>
    /// <param name="token">
    /// CancellationToken zum sofortigen Abbruch.
    /// </param>
    Task StreamAsync(
        string                                            filePath,
        Func<BbpNeoMassnahmeRaw, CancellationToken, Task> onMassnahme,
        CancellationToken                                 token);
}