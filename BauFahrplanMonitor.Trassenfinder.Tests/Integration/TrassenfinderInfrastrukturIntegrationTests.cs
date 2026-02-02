using BauFahrplanMonitor.Trassenfinder.Generated;
using BauFahrplanMonitor.Trassenfinder.Services;
using Xunit.Abstractions;

namespace BauFahrplanMonitor.Trassenfinder.Tests.Integration;

public sealed class TrassenfinderInfrastrukturIntegrationTests {
    private readonly ITestOutputHelper _testOutputHelper;
    public TrassenfinderInfrastrukturIntegrationTests(ITestOutputHelper testOutputHelper) {
        this._testOutputHelper = testOutputHelper;
    }
    [Fact]
    public async Task Infrastruktur_api_returns_real_data() {
        // Arrange
        var baseUrl = Environment.GetEnvironmentVariable("TRASSENFINDER_BASEURL");
        var token   = Environment.GetEnvironmentVariable("TRASSENFINDER_TOKEN");

        Assert.False(string.IsNullOrWhiteSpace(baseUrl), "TRASSENFINDER_BASEURL fehlt");
        Assert.False(string.IsNullOrWhiteSpace(token), "TRASSENFINDER_TOKEN fehlt");

        var httpClient = new HttpClient {
            BaseAddress = new Uri(baseUrl)
        };
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var client  = new TrassenfinderClient(httpClient);
        var service = new TrassenfinderService(client);

        // Act
        var result = await service.LadeInfrastrukturAsync();

        // Assert / Inspect
        var list = result.Take(10).ToList();

        Assert.NotEmpty(list);

        foreach (var item in list) {
            _testOutputHelper.WriteLine($"{item.Id} | {item.Bezeichnung}");
        }
    }

    [Fact]
    public async Task Infrastruktur_list_and_detail_return_real_data() {
        // --------------------------------------------------
        // Arrange
        // --------------------------------------------------
        var baseUrl = Environment.GetEnvironmentVariable("TRASSENFINDER_BASEURL");
        var token   = Environment.GetEnvironmentVariable("TRASSENFINDER_TOKEN");

        Assert.False(string.IsNullOrWhiteSpace(baseUrl), "TRASSENFINDER_BASEURL fehlt");
        Assert.False(string.IsNullOrWhiteSpace(token), "TRASSENFINDER_TOKEN fehlt");

        var httpClient = new HttpClient {
            BaseAddress = new Uri(baseUrl)
        };

        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var client  = new TrassenfinderClient(httpClient);
        var service = new TrassenfinderService(client);

        // --------------------------------------------------
        // Act – Liste
        // --------------------------------------------------
        var list = (await service.LadeInfrastrukturAsync()).ToList();

        Assert.NotEmpty(list);

        var first = list.First();

        _testOutputHelper.WriteLine(
            $"LISTE: {first.Id} | {first.Bezeichnung} | Jahr={first.FahrplanJahr}"
        );

        // --------------------------------------------------
        // Act – Detail (immer erstes Element)
        // --------------------------------------------------
        var infrastrukturId = long.Parse(first.Id);
        var detail          = await service.LadeInfrastrukturAsync(infrastrukturId);

        // --------------------------------------------------
        // Assert – Infrastruktur
        // --------------------------------------------------
        Assert.NotNull(detail);
        Assert.False(string.IsNullOrWhiteSpace(detail.Anzeigename));

        _testOutputHelper.WriteLine(
            $"DETAIL: {detail.Id} | {detail.Anzeigename}"
        );

        // --------------------------------------------------
        // Assert – Betriebsstellen
        // --------------------------------------------------
        Assert.NotNull(detail.Betriebsstellen);
        Assert.NotEmpty(detail.Betriebsstellen);

        var bs = detail.Betriebsstellen.First();

        _testOutputHelper.WriteLine(
            $"BS: {bs.Ds100} | {bs.Name} | {bs.Breite},{bs.Laenge}"
        );

        Assert.False(string.IsNullOrWhiteSpace(bs.Ds100));
        Assert.False(string.IsNullOrWhiteSpace(bs.Name));
        Assert.NotEqual(0, bs.Breite);
        Assert.NotEqual(0, bs.Laenge);

        // --------------------------------------------------
        // Assert – Triebfahrzeuge (Stammdaten)
        // --------------------------------------------------
        Assert.NotNull(detail.Triebfahrzeuge);
        Assert.NotEmpty(detail.Triebfahrzeuge);

        var tfz = detail.Triebfahrzeuge.First();

        _testOutputHelper.WriteLine(
            $"TFZ: {tfz.Baureihe} | {tfz.Bezeichnung} | Kennung={tfz.KennungWert}"
        );

        Assert.False(string.IsNullOrWhiteSpace(tfz.Baureihe));
        Assert.False(string.IsNullOrWhiteSpace(tfz.Bezeichnung));
        Assert.True(tfz.KennungWert > 0);
    }
}