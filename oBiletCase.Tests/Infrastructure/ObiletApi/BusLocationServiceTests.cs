using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using oBiletCase.Application.Sessions;
using oBiletCase.Infrastructure.oBiletAPI;
using oBiletCase.Infrastructure.oBiletAPI.Contracts;

namespace oBiletCase.Tests.Infrastructure.ObiletApi;

public class BusLocationServiceTests
{
    private static readonly ObiletSession Session = new("sid-1", "did-1");

    [Fact]
    public async Task GetLocationsAsync_api_yanitini_id_ve_name_alanlariyla_doner()
    {
        var apiClient = new Mock<IObiletApiClient>();
        apiClient
            .Setup(c => c.GetBusLocationsAsync(Session, It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ObiletApiEnvelope<List<BusLocationDto>>
            {
                Status = "Success",
                Data =
                [
                    new BusLocationDto { Id = 349, Name = "İstanbul Avrupa" },
                    new BusLocationDto { Id = 356, Name = "Ankara" },
                ],
            });

        var service = BuildService(apiClient.Object);

        var locations = await service.GetLocationsAsync("user-1", null, "tr-TR", CancellationToken.None);

        Assert.Equal(2, locations.Count);
        Assert.Equal(349, locations[0].Id);
        Assert.Equal("İstanbul Avrupa", locations[0].Name);
        Assert.Equal(356, locations[1].Id);
        Assert.Equal("Ankara", locations[1].Name);
    }

    [Fact]
    public async Task GetLocationsAsync_arama_metnini_ve_dili_oldugu_gibi_apiye_iletir()
    {
        var apiClient = new Mock<IObiletApiClient>();
        apiClient
            .Setup(c => c.GetBusLocationsAsync(Session, "ankara", "tr-TR", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ObiletApiEnvelope<List<BusLocationDto>> { Status = "Success", Data = [] });

        var service = BuildService(apiClient.Object);

        await service.GetLocationsAsync("user-1", "ankara", "tr-TR", CancellationToken.None);

        apiClient.Verify(
            c => c.GetBusLocationsAsync(Session, "ankara", "tr-TR", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetLocationsAsync_en_US_dilini_obiletin_bekledigi_en_EN_degerine_cevirir()
    {
        var apiClient = new Mock<IObiletApiClient>();
        apiClient
            .Setup(c => c.GetBusLocationsAsync(Session, It.IsAny<string?>(), "en-EN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ObiletApiEnvelope<List<BusLocationDto>> { Status = "Success", Data = [] });

        var service = BuildService(apiClient.Object);

        await service.GetLocationsAsync("user-1", null, "en-US", CancellationToken.None);

        apiClient.Verify(
            c => c.GetBusLocationsAsync(Session, It.IsAny<string?>(), "en-EN", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetLocationsAsync_api_basarisiz_status_donerse_exception_firlatir()
    {
        var apiClient = new Mock<IObiletApiClient>();
        apiClient
            .Setup(c => c.GetBusLocationsAsync(Session, It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ObiletApiEnvelope<List<BusLocationDto>> { Status = "Timeout" });

        var service = BuildService(apiClient.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetLocationsAsync("user-1", null, "tr-TR", CancellationToken.None));
    }

    [Fact]
    public async Task GetLocationsAsync_filtresiz_sorguda_ikinci_cagirimda_apiye_gitmez_cache_ten_doner()
    {
        var apiClient = new Mock<IObiletApiClient>();
        apiClient
            .Setup(c => c.GetBusLocationsAsync(Session, null, "tr-TR", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ObiletApiEnvelope<List<BusLocationDto>>
            {
                Status = "Success",
                Data = [new BusLocationDto { Id = 349, Name = "İstanbul Avrupa" }],
            });

        var service = BuildService(apiClient.Object);

        await service.GetLocationsAsync("user-1", null, "tr-TR", CancellationToken.None);
        var second = await service.GetLocationsAsync("user-1", null, "tr-TR", CancellationToken.None);

        Assert.Single(second);
        apiClient.Verify(
            c => c.GetBusLocationsAsync(Session, null, "tr-TR", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetLocationsAsync_metinli_sorguda_her_seferinde_apiye_gider_cache_lenmez()
    {
        var apiClient = new Mock<IObiletApiClient>();
        apiClient
            .Setup(c => c.GetBusLocationsAsync(Session, "ankara", "tr-TR", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ObiletApiEnvelope<List<BusLocationDto>> { Status = "Success", Data = [] });

        var service = BuildService(apiClient.Object);

        await service.GetLocationsAsync("user-1", "ankara", "tr-TR", CancellationToken.None);
        await service.GetLocationsAsync("user-1", "ankara", "tr-TR", CancellationToken.None);

        apiClient.Verify(
            c => c.GetBusLocationsAsync(Session, "ankara", "tr-TR", It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    private static BusLocationService BuildService(IObiletApiClient apiClient)
    {
        var sessionAccessor = new Mock<IObiletSessionAccessor>();
        sessionAccessor
            .Setup(s => s.GetOrCreateSessionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Session);

        return new BusLocationService(
            apiClient, sessionAccessor.Object, new MemoryCache(new MemoryCacheOptions()), NullLogger<BusLocationService>.Instance);
    }
}
