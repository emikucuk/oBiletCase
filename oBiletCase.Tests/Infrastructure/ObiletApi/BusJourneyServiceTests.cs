using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using oBiletCase.Application.Journeys;
using oBiletCase.Application.Sessions;
using oBiletCase.Infrastructure.oBiletAPI;
using oBiletCase.Infrastructure.oBiletAPI.Contracts;

namespace oBiletCase.Tests.Infrastructure.ObiletApi;

public class BusJourneyServiceTests
{
    private static readonly ObiletSession Session = new("sid-1", "did-1");

    [Fact]
    public async Task GetJourneysAsync_sonuclari_kalkis_saatine_gore_artan_sekilde_siralar()
    {
        var earlyDeparture = CreateJourneyDto(id: 1, departure: new DateTime(2026, 9, 23, 8, 0, 0));
        var lateDeparture = CreateJourneyDto(id: 2, departure: new DateTime(2026, 9, 23, 20, 0, 0));

        var service = BuildService(lateDeparture, earlyDeparture);

        var journeys = await service.GetJourneysAsync("user-1", AnyCriteria, "tr-TR", CancellationToken.None);

        Assert.Equal([1, 2], journeys.Select(j => j.Id));
    }

    [Fact]
    public async Task GetJourneysAsync_api_yanitini_dogru_alanlarla_esler()
    {
        var dto = CreateJourneyDto(
            id: 1397550800,
            partnerId: 3578,
            partnerName: "Tokat Kale Seyahat",
            busType: "2+1",
            availableSeats: 41,
            departure: new DateTime(2026, 9, 23, 19, 15, 0),
            arrival: new DateTime(2026, 9, 23, 21, 15, 0),
            duration: TimeSpan.FromHours(2),
            priceAmount: 400.0m,
            currency: "TRY",
            cancellationOffsetHours: 6,
            partnerRating: 5.0m,
            mixedGenders: false,
            govIdRequired: true,
            description: "ZİLE BİLETLİ YOLCULARIMIZ TURHAL DURAĞINDAN SERVİS İLE DEVAM EDECEKLERDİR.",
            features: [new JourneyFeatureDto { Id = 10, Name = "Kablosuz Internet (WiFi)" }]);

        var service = BuildService(dto);

        var journey = (await service.GetJourneysAsync("user-1", AnyCriteria, "tr-TR", CancellationToken.None)).Single();

        Assert.Equal(3578, journey.PartnerId);
        Assert.Equal("Tokat Kale Seyahat", journey.PartnerName);
        Assert.Equal("2+1", journey.BusType);
        Assert.Equal(41, journey.AvailableSeats);
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
    public async Task GetJourneysAsync_bos_aciklamayi_null_olarak_esler()
    {
        var dto = CreateJourneyDto(id: 1, description: "");

        var service = BuildService(dto);

        var journey = (await service.GetJourneysAsync("user-1", AnyCriteria, "tr-TR", CancellationToken.None)).Single();

        Assert.Null(journey.Details.Description);
    }

    [Fact]
    public async Task GetJourneysAsync_bos_sonuc_donerse_bos_liste_doner()
    {
        var service = BuildService();

        var journeys = await service.GetJourneysAsync("user-1", AnyCriteria, "tr-TR", CancellationToken.None);

        Assert.Empty(journeys);
    }

    [Fact]
    public async Task GetJourneysAsync_api_basarisiz_status_donerse_exception_firlatir()
    {
        var apiClient = new Mock<IObiletApiClient>();
        apiClient
            .Setup(c => c.GetBusJourneysAsync(Session, It.IsAny<JourneySearchCriteria>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ObiletApiEnvelope<List<JourneyDto>> { Status = "InvalidRoute" });

        var service = BuildServiceWithApiClient(apiClient.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetJourneysAsync("user-1", AnyCriteria, "tr-TR", CancellationToken.None));
    }

    private static JourneySearchCriteria AnyCriteria { get; } =
        new(OriginLocationId: 349, DestinationLocationId: 356, DepartureDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)));

    private static JourneyDto CreateJourneyDto(
        long id,
        int partnerId = 1,
        string partnerName = "Test Turizm",
        string busType = "2+1",
        int availableSeats = 40,
        DateTime? departure = null,
        DateTime? arrival = null,
        TimeSpan? duration = null,
        decimal priceAmount = 100m,
        string currency = "TRY",
        int? cancellationOffsetHours = null,
        decimal? partnerRating = null,
        bool mixedGenders = false,
        bool govIdRequired = true,
        string? description = null,
        List<JourneyFeatureDto>? features = null) => new()
    {
        Id = id,
        PartnerId = partnerId,
        PartnerName = partnerName,
        BusType = busType,
        AvailableSeats = availableSeats,
        CancellationOffsetHours = cancellationOffsetHours,
        PartnerRating = partnerRating,
        Features = features ?? [],
        Journey = new JourneyDetailDto
        {
            Origin = "Kalkış Otogarı",
            Destination = "Varış Otogarı",
            Departure = departure ?? new DateTime(2026, 9, 23, 10, 0, 0),
            Arrival = arrival ?? new DateTime(2026, 9, 23, 12, 0, 0),
            Duration = duration ?? TimeSpan.FromHours(2),
            Currency = currency,
            InternetPrice = priceAmount,
            Description = description,
            Policy = new JourneyPolicyDto { MixedGenders = mixedGenders, GovIdRequired = govIdRequired },
        },
    };

    private static BusJourneyService BuildService(params JourneyDto[] dtos)
    {
        var apiClient = new Mock<IObiletApiClient>();
        apiClient
            .Setup(c => c.GetBusJourneysAsync(Session, It.IsAny<JourneySearchCriteria>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ObiletApiEnvelope<List<JourneyDto>> { Status = "Success", Data = dtos.ToList() });

        return BuildServiceWithApiClient(apiClient.Object);
    }

    private static BusJourneyService BuildServiceWithApiClient(IObiletApiClient apiClient)
    {
        var sessionAccessor = new Mock<IObiletSessionAccessor>();
        sessionAccessor
            .Setup(s => s.GetOrCreateSessionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Session);

        return new BusJourneyService(apiClient, sessionAccessor.Object, NullLogger<BusJourneyService>.Instance);
    }
}
