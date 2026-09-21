using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using oBiletCase.Infrastructure.oBiletAPI;
using oBiletCase.Infrastructure.oBiletAPI.Contracts;

namespace oBiletCase.Tests.Infrastructure.ObiletApi;

public class ObiletSessionAccessorTests
{
    private static readonly ObiletApiEnvelope<DeviceSessionDto> SuccessEnvelope = new()
    {
        Status = "Success",
        Data = new DeviceSessionDto { SessionId = "sid-1", DeviceId = "did-1" },
    };

    [Fact]
    public async Task GetOrCreateSessionAsync_ilk_cagirimda_apiden_session_doner()
    {
        var apiClient = new Mock<IObiletApiClient>();
        apiClient.Setup(c => c.GetSessionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(SuccessEnvelope);

        var accessor = BuildAccessor(apiClient.Object);

        var session = await accessor.GetOrCreateSessionAsync("user-1", CancellationToken.None);

        Assert.Equal("sid-1", session.SessionId);
        Assert.Equal("did-1", session.DeviceId);
        apiClient.Verify(c => c.GetSessionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateSessionAsync_ayni_kullanici_icin_ikinci_cagirimda_cache_ten_doner()
    {
        var apiClient = new Mock<IObiletApiClient>();
        apiClient.Setup(c => c.GetSessionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(SuccessEnvelope);

        var accessor = BuildAccessor(apiClient.Object);

        var first = await accessor.GetOrCreateSessionAsync("user-1", CancellationToken.None);
        var second = await accessor.GetOrCreateSessionAsync("user-1", CancellationToken.None);

        Assert.Equal(first, second);
        apiClient.Verify(c => c.GetSessionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateSessionAsync_farkli_kullanicilar_icin_izole_session_doner()
    {
        var apiClient = new Mock<IObiletApiClient>();
        apiClient.SetupSequence(c => c.GetSessionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ObiletApiEnvelope<DeviceSessionDto>
            {
                Status = "Success",
                Data = new DeviceSessionDto { SessionId = "sid-user1", DeviceId = "did-user1" },
            })
            .ReturnsAsync(new ObiletApiEnvelope<DeviceSessionDto>
            {
                Status = "Success",
                Data = new DeviceSessionDto { SessionId = "sid-user2", DeviceId = "did-user2" },
            });

        var accessor = BuildAccessor(apiClient.Object);

        var user1Session = await accessor.GetOrCreateSessionAsync("user-1", CancellationToken.None);
        var user2Session = await accessor.GetOrCreateSessionAsync("user-2", CancellationToken.None);

        Assert.NotEqual(user1Session.SessionId, user2Session.SessionId);
        apiClient.Verify(c => c.GetSessionAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetOrCreateSessionAsync_api_basarisiz_status_donerse_exception_firlatir()
    {
        var apiClient = new Mock<IObiletApiClient>();
        apiClient.Setup(c => c.GetSessionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ObiletApiEnvelope<DeviceSessionDto> { Status = "InvalidLocation" });

        var accessor = BuildAccessor(apiClient.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => accessor.GetOrCreateSessionAsync("user-1", CancellationToken.None));
    }

    [Fact]
    public async Task GetOrCreateSessionAsync_api_null_donerse_exception_firlatir()
    {
        var apiClient = new Mock<IObiletApiClient>();
        apiClient.Setup(c => c.GetSessionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ObiletApiEnvelope<DeviceSessionDto>?)null);

        var accessor = BuildAccessor(apiClient.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => accessor.GetOrCreateSessionAsync("user-1", CancellationToken.None));
    }

    private static ObiletSessionAccessor BuildAccessor(IObiletApiClient apiClient) =>
        new(apiClient, new MemoryCache(new MemoryCacheOptions()), NullLogger<ObiletSessionAccessor>.Instance);
}
