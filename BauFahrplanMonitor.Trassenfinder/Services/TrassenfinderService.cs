using BauFahrplanMonitor.Core.Domain.Infrastruktur;
using BauFahrplanMonitor.Core.Domain.Trassen;
using BauFahrplanMonitor.Trassenfinder.Generated;
using BauFahrplanMonitor.Trassenfinder.Mapping;
using Infrastruktur = BauFahrplanMonitor.Core.Domain.Infrastruktur.Infrastruktur;


namespace BauFahrplanMonitor.Trassenfinder.Services;

public sealed class TrassenfinderService : ITrassenfinderService {
    private readonly TrassenfinderClient _client;

    public TrassenfinderService(TrassenfinderClient client) {
        _client = client;
    }

    public async Task<IEnumerable<Trasse>> SucheAsync(
        TrassenSucheParameter parameter) {
        // TODO: echter API-Call (kommt als Nächstes)
        return Enumerable.Empty<Trasse>();
    }
    public async Task<IEnumerable<InfrastrukturElement>> LadeInfrastrukturAsync() {
        var dtos = await _client.Get_infrastrukturenAsync();
        return dtos.Select(TrassenfinderInfrastrukturMapper.Map);
    }

    public async Task<Infrastruktur> LadeInfrastrukturAsync(long infrastrukturId)
    {
        var dto = await _client.Get_infrastrukturAsync(infrastrukturId);
        return TrassenfinderInfrastrukturMapper.Map(dto);
    }

}