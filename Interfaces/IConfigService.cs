using BauFahrplanMonitor.Configuration;

namespace BauFahrplanMonitor.Interfaces;

public interface IConfigService {
    AppConfig Raw        { get; }
    AppConfig Effective  { get; }
    string    SessionKey { get; }
}