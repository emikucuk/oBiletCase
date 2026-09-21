using System.Net;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using oBiletCase.Application.Journeys;
using oBiletCase.Infrastructure.oBiletAPI;
using oBiletCase.Tests.TestSupport;

namespace oBiletCase.Tests.Infrastructure.ObiletApi;

public class BusJourneyServiceTests
{
    private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json"),
    };

    private const string SessionResponseJson =
        """{"status":"Success","data":{"session-id":"sid-1","device-id":"did-1"}}""";

    // Alan adları/şekli docs/seferler.json'daki gerçek, canlı GetBusJourneys yanıtından alınmıştır (2026-09-23).
    private const string JourneysResponseJson =
        """
        {
          "status": "Success",
          "data": [
            {
              "id": 1397550800,
              "partner-id": 3578,
              "partner-name": "Tokat Kale Seyahat",
              "bus-type": "2+1",
              "available-seats": 41,
              "cancellation-offset": 6,
              "partner-rating": 5.0,
              "journey": {
                "origin": "Esenyurt Balıkyolu Otobüs Kalkış-Varış Noktası",
                "destination": "Sultanbeyli Otogarı",
                "departure": "2026-09-23T19:15:00",
                "arrival": "2026-09-23T21:15:00",
                "duration": "02:00:00",
                "currency": "TRY",
                "internet-price": 400.0,
                "description": "ZİLE BİLETLİ YOLCULARIMIZ TURHAL DURAĞINDAN SERVİS İLE DEVAM EDECEKLERDİR.",
                "policy": { "mixed-genders": false, "gov-id": true }
              },
              "features": [
                {"id": 7, "priority": 13, "name": "220 Voltluk Priz"},
                {"id": 10, "priority": 10, "name": "Kablosuz Internet (WiFi)"}
              ]
            },
            {
              "id": 1375766754,
              "partner-id": 4021,
              "partner-name": "Rıdvan Ekinci Doğu Kars",
              "bus-type": "2+1",
              "available-seats": 35,
              "journey": {
                "origin": "İstanbul Avrupa",
                "destination": "İstanbul Anadolu",
                "departure": "2026-09-23T15:57:00",
                "arrival": "2026-09-23T17:27:00",
                "duration": "01:30:00",
                "currency": "TRY",
                "internet-price": 400.0
              },
              "features": []
            }
          ],
          "message": null,
          "user-message": null,
          "api-request-id": null,
          "controller": "JourneyController"
        }
        """;

    [Fact]
    public async Task GetJourneysAsync_api_yanitini_kalkis_saatine_gore_siralar()
    {
        var service = BuildService(new StubHttpMessageHandler((request, _) =>
            request.RequestUri!.AbsolutePath.Contains("getsession")
                ? JsonResponse(SessionResponseJson)
                : JsonResponse(JourneysResponseJson)));

        var criteria = new JourneySearchCriteria(349, 350, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var journeys = await service.GetJourneysAsync("user-1", criteria, "tr-TR", CancellationToken.None);

        Assert.Equal(2, journeys.Count);
        Assert.Equal(1375766754, journeys[0].Id);
        Assert.Equal(1397550800, journeys[1].Id);
        Assert.True(journeys[0].Departure < journeys[1].Departure);
    }

    [Fact]
    public async Task GetJourneysAsync_api_yanitini_dogru_alanlarla_esler()
    {
        var service = BuildService(new StubHttpMessageHandler((request, _) =>
            request.RequestUri!.AbsolutePath.Contains("getsession")
                ? JsonResponse(SessionResponseJson)
                : JsonResponse(JourneysResponseJson)));

        var criteria = new JourneySearchCriteria(349, 350, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var journeys = await service.GetJourneysAsync("user-1", criteria, "tr-TR", CancellationToken.None);

        var journey = journeys.Single(j => j.Id == 1397550800);

        Assert.Equal(3578, journey.PartnerId);
        Assert.Equal("Tokat Kale Seyahat", journey.PartnerName);
        Assert.Equal("2+1", journey.BusType);
        Assert.Equal(41, journey.AvailableSeats);
        Assert.Equal("Esenyurt Balıkyolu Otobüs Kalkış-Varış Noktası", journey.OriginStopName);
        Assert.Equal("Sultanbeyli Otogarı", journey.DestinationStopName);
        Assert.Equal(new DateTime(2026, 9, 23, 19, 15, 0), journey.Departure);
        Assert.Equal(TimeSpan.FromHours(2), journey.Duration);
        Assert.Equal("TRY", journey.Currency);
        Assert.Equal(400.0m, journey.PriceAmount);
        Assert.Contains(journey.Features, f => f is { Id: 10, Name: "Kablosuz Internet (WiFi)" });

        Assert.Equal("ZİLE BİLETLİ YOLCULARIMIZ TURHAL DURAĞINDAN SERVİS İLE DEVAM EDECEKLERDİR.", journey.Details.Description);
        Assert.Equal(6, journey.Details.CancellationOffsetHours);
        Assert.Equal(5.0m, journey.Details.PartnerRating);
        Assert.False(journey.Details.MixedGendersAllowed);
        Assert.True(journey.Details.GovIdRequired);
    }

    [Fact]
    public async Task GetJourneysAsync_bos_aciklama_null_olarak_esler()
    {
        var service = BuildService(new StubHttpMessageHandler((request, _) =>
            request.RequestUri!.AbsolutePath.Contains("getsession")
                ? JsonResponse(SessionResponseJson)
                : JsonResponse(JourneysResponseJson)));

        var criteria = new JourneySearchCriteria(349, 350, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var journeys = await service.GetJourneysAsync("user-1", criteria, "tr-TR", CancellationToken.None);

        var journey = journeys.Single(j => j.Id == 1375766754);

        Assert.Null(journey.Details.Description);
    }

    [Fact]
    public async Task GetJourneysAsync_istek_govdesi_dogru_sema_ile_gonderilir()
    {
        HttpRequestMessage? capturedRequest = null;
        string? capturedBody = null;

        var service = BuildService(new StubHttpMessageHandler((request, ct) =>
        {
            if (request.RequestUri!.AbsolutePath.Contains("getsession"))
            {
                return JsonResponse(SessionResponseJson);
            }

            capturedRequest = request;
            capturedBody = request.Content!.ReadAsStringAsync(ct).GetAwaiter().GetResult();
            return JsonResponse("""{"status":"Success","data":[]}""");
        }));

        var criteria = new JourneySearchCriteria(349, 356, new DateOnly(2026, 9, 25));
        await service.GetJourneysAsync("user-1", criteria, "tr-TR", CancellationToken.None);

        Assert.Equal("/api/journey/getbusjourneys", capturedRequest!.RequestUri!.AbsolutePath);
        Assert.Contains("\"device-session\":{\"session-id\":\"sid-1\",\"device-id\":\"did-1\"}", capturedBody);
        Assert.Contains("\"origin-id\":349", capturedBody);
        Assert.Contains("\"destination-id\":356", capturedBody);
        Assert.Contains("\"departure-date\":\"2026-09-25T00:00:00\"", capturedBody);
    }

    [Fact]
    public async Task GetJourneysAsync_bos_sonuc_donerse_bos_liste_doner()
    {
        var service = BuildService(new StubHttpMessageHandler((request, _) =>
            request.RequestUri!.AbsolutePath.Contains("getsession")
                ? JsonResponse(SessionResponseJson)
                : JsonResponse("""{"status":"Success","data":[]}""")));

        var criteria = new JourneySearchCriteria(1, 2, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var journeys = await service.GetJourneysAsync("user-1", criteria, "tr-TR", CancellationToken.None);

        Assert.Empty(journeys);
    }

    [Fact]
    public async Task GetJourneysAsync_api_basarisiz_status_donerse_exception_firlatir()
    {
        var service = BuildService(new StubHttpMessageHandler((request, _) =>
            request.RequestUri!.AbsolutePath.Contains("getsession")
                ? JsonResponse(SessionResponseJson)
                : JsonResponse("""{"status":"InvalidRoute","data":null,"message":"gecersiz rota"}""")));

        var criteria = new JourneySearchCriteria(1, 2, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetJourneysAsync("user-1", criteria, "tr-TR", CancellationToken.None));
    }

    private static BusJourneyService BuildService(HttpMessageHandler handler)
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

        return new BusJourneyService(apiClient, sessionAccessor, NullLogger<BusJourneyService>.Instance);
    }
}
