using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using oBiletCase.Application.Journeys;
using oBiletCase.Web.AppUser;
using oBiletCase.Web.Controllers;
using oBiletCase.Web.Models.Journeys;

namespace oBiletCase.Tests.Web.Controllers;

public class JourneysControllerTests
{
    private static readonly DateOnly DepartureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

    [Fact]
    public async Task Index_gecersiz_kriterle_home_a_yonlendirir_ve_servisi_hic_cagirmaz()
    {
        var busJourneyService = new Mock<IBusJourneyService>();
        var controller = BuildController(busJourneyService.Object);

        var result = await controller.Index(
            originId: 1, originName: "A", destinationId: 1, destinationName: "A", departureDate: DepartureDate, CancellationToken.None);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
        Assert.True((bool)redirect.RouteValues!["invalidSearch"]!);

        busJourneyService.Verify(
            s => s.GetJourneysAsync(It.IsAny<string>(), It.IsAny<JourneySearchCriteria>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Index_basarili_sonucta_journey_alanlarini_dogru_esler()
    {
        var journey = new Journey(
            Id: 42,
            PartnerId: 330,
            PartnerName: "Test Turizm",
            OriginStopName: "Kalkış Otogarı",
            DestinationStopName: "Varış Otogarı",
            Departure: new DateTime(2026, 9, 23, 10, 0, 0),
            Arrival: new DateTime(2026, 9, 23, 12, 0, 0),
            Duration: TimeSpan.FromHours(2),
            BusType: "2+1",
            AvailableSeats: 40,
            PriceAmount: 250m,
            Currency: "TRY",
            Features: [new JourneyFeature(10, "Kablosuz Internet (WiFi)")],
            Details: new JourneyDetails(
                Description: "Not",
                CancellationOffsetHours: 6,
                PartnerRating: 5.0m,
                MixedGendersAllowed: false,
                GovIdRequired: true));

        var busJourneyService = new Mock<IBusJourneyService>();
        busJourneyService
            .Setup(s => s.GetJourneysAsync("user-1", It.IsAny<JourneySearchCriteria>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([journey]);

        var controller = BuildController(busJourneyService.Object);

        var result = await controller.Index(
            originId: 349, originName: "İstanbul Avrupa", destinationId: 356, destinationName: "Ankara",
            departureDate: DepartureDate, CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<JourneyResultsViewModel>(view.Model);

        Assert.False(model.HasLoadError);
        var card = Assert.Single(model.Journeys);
        Assert.Equal("Test Turizm", card.PartnerName);
        Assert.Contains("330", card.PartnerLogoUrl);
        Assert.Equal("Kalkış Otogarı", card.OriginStopName);
        Assert.Equal(250m, card.PriceAmount);
        Assert.Contains(card.Features, f => f.Name == "Kablosuz Internet (WiFi)" && f.IconUrl.Contains("10"));
        Assert.Equal(5.0m, card.Details.PartnerRating);
        Assert.Contains("349-356", card.ObiletUrl);
        Assert.Contains("42", card.ObiletUrl);
    }

    [Fact]
    public async Task Index_servis_beklenen_bir_hata_firlatirsa_hata_bayragiyla_view_doner()
    {
        var busJourneyService = new Mock<IBusJourneyService>();
        busJourneyService
            .Setup(s => s.GetJourneysAsync(It.IsAny<string>(), It.IsAny<JourneySearchCriteria>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Obilet sefer listesi alınamadı."));

        var controller = BuildController(busJourneyService.Object);

        var result = await controller.Index(
            originId: 349, originName: "İstanbul Avrupa", destinationId: 356, destinationName: "Ankara",
            departureDate: DepartureDate, CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<JourneyResultsViewModel>(view.Model);

        Assert.True(model.HasLoadError);
        Assert.Empty(model.Journeys);
    }

    [Fact]
    public async Task Index_bos_sonucta_hata_olmadan_bos_liste_doner()
    {
        var busJourneyService = new Mock<IBusJourneyService>();
        busJourneyService
            .Setup(s => s.GetJourneysAsync(It.IsAny<string>(), It.IsAny<JourneySearchCriteria>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var controller = BuildController(busJourneyService.Object);

        var result = await controller.Index(
            originId: 349, originName: "İstanbul Avrupa", destinationId: 356, destinationName: "Ankara",
            departureDate: DepartureDate, CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<JourneyResultsViewModel>(view.Model);

        Assert.False(model.HasLoadError);
        Assert.Empty(model.Journeys);
    }

    private static JourneysController BuildController(IBusJourneyService busJourneyService)
    {
        var appUserContext = new Mock<IAppUserContext>();
        appUserContext.SetupGet(c => c.AppUserId).Returns("user-1");

        return new JourneysController(
            busJourneyService,
            new JourneySearchCriteriaValidator(),
            appUserContext.Object,
            NullLogger<JourneysController>.Instance);
    }
}
