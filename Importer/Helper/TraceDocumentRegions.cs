using BauFahrplanMonitor.Importer.Dto.Fplo;
using BauFahrplanMonitor.Importer.Dto.Shared;
using NLog;

namespace BauFahrplanMonitor.Importer.Helper;

public static class DebugTraceHelper {
    public static void TraceDocumentRegions(
        Logger             logger,
        string             phase,
        FploXmlDocumentDto dto) {
        logger.Debug(
            "[TRACE:{0}] Datei='{1}', Region='{2}', Masterniederlassung='{3}'",
            phase,
            dto.Document.Dateiname,
            dto.Document.Region,
            ((SharedDocumentDto)dto.Document).MasterRegion);
    }
}