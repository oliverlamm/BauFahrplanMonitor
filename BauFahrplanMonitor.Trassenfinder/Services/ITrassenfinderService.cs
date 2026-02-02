using BauFahrplanMonitor.Core.Domain.Infrastruktur;
using BauFahrplanMonitor.Core.Domain.Trassen;
using Infrastruktur = BauFahrplanMonitor.Core.Domain.Infrastruktur.Infrastruktur;

namespace BauFahrplanMonitor.Trassenfinder.Services;

public interface ITrassenfinderService {
    Task<IEnumerable<Trasse>> SucheAsync(
        TrassenSucheParameter parameter);
    Task<IEnumerable<InfrastrukturElement>> LadeInfrastrukturAsync();
    Task<Infrastruktur> LadeInfrastrukturAsync(long infrastrukturId);
}