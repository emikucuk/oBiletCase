using System.Net;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using oBiletCase.Infrastructure.oBiletAPI;
using oBiletCase.Tests.TestSupport;

namespace oBiletCase.Tests.Infrastructure.ObiletApi;

public class BusLocationServiceTests
{
    private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json"),
    };

    private const string SessionResponseJson =
        """{"status":"Success","data":{"session-id":"sid-1","device-id":"did-1"}}""";

    [Fact]
    public async Task GetLocationsAsync_api_yanitini_id_ve_name_alanlariyla_doner()
    {
        var service = BuildService(new StubHttpMessageHandler((request, ct) =>
            request.RequestUri!.AbsolutePath.Contains("getsession")
                ? JsonResponse(SessionResponseJson)
                : JsonResponse(
                    """{"status":"Success","data":[{"id":349,"parent-id":250,"type":"Town","name":"İstanbul Avrupa"},{"id":356,"name":"Ankara"}]}""")));

        var locations = await service.GetLocationsAsync("user-1", null, "tr-TR", CancellationToken.None);

        Assert.Equal(2, locations.Count);
        Assert.Equal(349, locations[0].Id);
        Assert.Equal("İstanbul Avrupa", locations[0].Name);
        Assert.Equal(356, locations[1].Id);
        Assert.Equal("Ankara", locations[1].Name);
    }

    [Fact]
    public async Task GetLocationsAsync_istek_govdesi_dokumante_edilmis_semayla_birebir_esler()
    {
        HttpRequestMessage? capturedLocationsRequest = null;
        string? capturedLocationsBody = null;

        var service = BuildService(new StubHttpMessageHandler((request, ct) =>
        {
            if (request.RequestUri!.AbsolutePath.Contains("getsession"))
            {
                return JsonResponse(SessionResponseJson);
            }

            capturedLocationsRequest = request;
            capturedLocationsBody = request.Content!.ReadAsStringAsync(ct).GetAwaiter().GetResult();
            return JsonResponse("""{"status":"Success","data":[]}""");
        }));

        await service.GetLocationsAsync("user-1", "ankara", "tr-TR", CancellationToken.None);

        Assert.Equal("/api/location/getbuslocations", capturedLocationsRequest!.RequestUri!.AbsolutePath);
        Assert.Contains("\"data\":\"ankara\"", capturedLocationsBody);
        Assert.Contains("\"device-session\":{\"session-id\":\"sid-1\",\"device-id\":\"did-1\"}", capturedLocationsBody);
        Assert.Contains("\"language\":\"tr-TR\"", capturedLocationsBody);
    }

    [Fact]
    public async Task GetLocationsAsync_data_null_ise_arama_filtresi_gonderilmez()
    {
        string? capturedLocationsBody = null;

        var service = BuildService(new StubHttpMessageHandler((request, ct) =>
        {
            if (request.RequestUri!.AbsolutePath.Contains("getsession"))
            {
                return JsonResponse(SessionResponseJson);
            }

            capturedLocationsBody = request.Content!.ReadAsStringAsync(ct).GetAwaiter().GetResult();
            return JsonResponse("""{"status":"Success","data":[]}""");
        }));

        await service.GetLocationsAsync("user-1", null, "tr-TR", CancellationToken.None);

        Assert.Contains("\"data\":null", capturedLocationsBody);
    }

    [Fact]
    public async Task GetLocationsAsync_api_basarisiz_status_donerse_exception_firlatir()
    {
        var service = BuildService(new StubHttpMessageHandler((request, ct) =>
        {
            if (request.RequestUri!.AbsolutePath.Contains("getsession"))
            {
                return JsonResponse(SessionResponseJson);
            }

            return JsonResponse("""{"status":"Timeout","data":null,"message":"zaman asimi"}""");
        }));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetLocationsAsync("user-1", null, "tr-TR", CancellationToken.None));
    }

    private static BusLocationService BuildService(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://obilet.test/api/") };
        var factory = new StubHttpClientFactory(httpClient);
        var options = Options.Create(new oBiletAPIOptions
        {
            BaseUrl = "https://obilet.test/api",
            ApiClientToken = "test-token",
        });

        var apiClient = new ObiletApiClient(factory, options, NullLogger<ObiletApiClient>.Instance);
        var sessionAccessor = new ObiletSessionAccessor(
            apiClient, new MemoryCache(new MemoryCacheOptions()), NullLogger<ObiletSessionAccessor>.Instance);

        return new BusLocationService(apiClient, sessionAccessor, NullLogger<BusLocationService>.Instance);
    }
}
