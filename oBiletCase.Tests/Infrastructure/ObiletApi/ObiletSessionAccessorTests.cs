using System.Net;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using oBiletCase.Infrastructure.oBiletAPI;
using oBiletCase.Tests.TestSupport;

namespace oBiletCase.Tests.Infrastructure.ObiletApi;

/// <summary>
/// <see cref="ObiletSessionAccessor"/>'ın gerçek Obilet API'sine bağımlı
/// olmadan, sahte bir HTTP handler üzerinden doğrulanan testleri.
/// </summary>
public class ObiletSessionAccessorTests
{
    private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json"),
    };

    [Fact]
    public async Task GetOrCreateSessionAsync_ilk_cagirimda_apiden_session_doner()
    {
        var callCount = 0;
        var handler = new StubHttpMessageHandler((request, ct) =>
        {
            callCount++;
            return JsonResponse("""{"status":"Success","data":{"session-id":"sid-1","device-id":"did-1"}}""");
        });

        var accessor = BuildAccessor(handler);

        var session = await accessor.GetOrCreateSessionAsync("user-1", CancellationToken.None);

        Assert.Equal("sid-1", session.SessionId);
        Assert.Equal("did-1", session.DeviceId);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task GetOrCreateSessionAsync_istek_govdesi_canli_apiye_karsi_dogrulanmis_semayla_birebir_esler()
    {
        // Bu şema 2026-09-20'de gerçek Obilet API'sine karşı curl ile doğrulanmıştır
        // (bkz. GetSessionRequestDto.cs remarks). docs/ altındaki PDF'in örneği
        // canlı API tarafından reddedilir; burada PDF değil, gerçekte çalışan
        // gövde regresyona karşı sabitlenir.
        HttpRequestMessage? capturedRequest = null;
        string? capturedBody = null;
        var handler = new StubHttpMessageHandler((request, ct) =>
        {
            capturedRequest = request;
            capturedBody = request.Content!.ReadAsStringAsync(ct).GetAwaiter().GetResult();
            return JsonResponse("""{"status":"Success","data":{"session-id":"sid-1","device-id":"did-1"}}""");
        });

        var accessor = BuildAccessor(handler);
        await accessor.GetOrCreateSessionAsync("user-1", CancellationToken.None);

        Assert.Equal("/api/client/getsession", capturedRequest!.RequestUri!.AbsolutePath);
        Assert.Contains("\"type\":1", capturedBody);
        Assert.Contains("\"connection\":{\"ip-address\":", capturedBody);
        Assert.Contains("\"port\":\"0\"", capturedBody);
        Assert.Contains("\"browser\":{\"name\":", capturedBody);
        Assert.DoesNotContain("\"application\":", capturedBody); // PDF'teki eski şemadan kalma alan olmamalı
    }

    [Fact]
    public async Task GetOrCreateSessionAsync_ayni_kullanici_icin_ikinci_cagirimda_cache_ten_doner()
    {
        var callCount = 0;
        var handler = new StubHttpMessageHandler((request, ct) =>
        {
            callCount++;
            return JsonResponse(
                "{\"status\":\"Success\",\"data\":{\"session-id\":\"sid-" + callCount + "\",\"device-id\":\"did-" + callCount + "\"}}");
        });

        var accessor = BuildAccessor(handler);

        var first = await accessor.GetOrCreateSessionAsync("user-1", CancellationToken.None);
        var second = await accessor.GetOrCreateSessionAsync("user-1", CancellationToken.None);

        Assert.Equal(first, second);
        Assert.Equal(1, callCount); // ikinci çağrı API'ye gitmemeli, cache'ten dönmeli
    }

    [Fact]
    public async Task GetOrCreateSessionAsync_farkli_kullanicilar_icin_izole_session_doner()
    {
        var callCount = 0;
        var handler = new StubHttpMessageHandler((request, ct) =>
        {
            callCount++;
            return JsonResponse(
                "{\"status\":\"Success\",\"data\":{\"session-id\":\"sid-" + callCount + "\",\"device-id\":\"did-" + callCount + "\"}}");
        });

        var accessor = BuildAccessor(handler);

        var user1Session = await accessor.GetOrCreateSessionAsync("user-1", CancellationToken.None);
        var user2Session = await accessor.GetOrCreateSessionAsync("user-2", CancellationToken.None);

        Assert.NotEqual(user1Session.SessionId, user2Session.SessionId);
        Assert.Equal(2, callCount); // farklı kullanıcılar ayrı ayrı API'ye gitmeli
    }

    [Fact]
    public async Task GetOrCreateSessionAsync_api_basarisiz_status_donerse_exception_firlatir()
    {
        var handler = new StubHttpMessageHandler((request, ct) =>
            JsonResponse("""{"status":"InvalidLocation","data":null,"message":"gecersiz"}"""));

        var accessor = BuildAccessor(handler);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => accessor.GetOrCreateSessionAsync("user-1", CancellationToken.None));
    }

    private static ObiletSessionAccessor BuildAccessor(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://obilet.test/api/") };
        var factory = new StubHttpClientFactory(httpClient);
        var options = Options.Create(new oBiletAPIOptions
        {
            BaseUrl = "https://obilet.test/api",
            ApiClientToken = "test-token",
        });

        var apiClient = new ObiletApiClient(
            factory, options, NullLogger<ObiletApiClient>.Instance);

        return new ObiletSessionAccessor(apiClient, new MemoryCache(new MemoryCacheOptions()), NullLogger<ObiletSessionAccessor>.Instance);
    }
}
