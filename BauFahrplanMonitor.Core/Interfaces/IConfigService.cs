using BauFahrplanMonitor.Core.Configuration;

namespace BauFahrplanMonitor.Core.Interfaces;

public interface IConfigService {
    AppConfig Raw        { get; }
    AppConfig Effective  { get; }
    string    SessionKey { get; }
}