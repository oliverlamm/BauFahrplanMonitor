using BauFahrplanMonitor.Services;

namespace BauFahrplanMonitor.Helpers {
    public class DatabaseCheckResult {
        public DatabaseService.DatabaseHealthStatus Status               { get; set; }
        public string                               Message              { get; set; }
        public int?                                 CurrentSchemaVersion { get; set; }

        public DatabaseCheckResult(DatabaseService.DatabaseHealthStatus status, string message, int? currentSchemaVersion) {
            Status               = status;
            Message              = message;
            CurrentSchemaVersion = currentSchemaVersion;
        }
    }
}