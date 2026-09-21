using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using oBiletCase.Application.Locations;
using oBiletCase.Web.AppUser;
using oBiletCase.Web.Controllers;
using oBiletCase.Web.Models.Locations;

namespace oBiletCase.Tests.Web.Controllers;

public class LocationsControllerTests
{
    [Fact]
    public async Task Search_basarili_yanitta_lokasyonlari_json_olarak_doner()
    {
        var busLocationService = new Mock<IBusLocationService>();
        busLocationService
            .Setup(s => s.GetLocationsAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new BusLocation(349, "İstanbul Avrupa")]);

        var controller = BuildController(busLocationService.Object);

        var result = Assert.IsType<JsonResult>(await controller.Search("istanbul", CancellationToken.None));
        var locations = Assert.IsAssignableFrom<IEnumerable<LocationOptionViewModel>>(result.Value);

        Assert.Single(locations);
    }

    [Fact]
    public async Task Search_api_hatasinda_502_doner_exception_firlatmaz()
    {
        var busLocationService = new Mock<IBusLocationService>();
        busLocationService
            .Setup(s => s.GetLocationsAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Obilet lokasyon servisi başarısız oldu."));

        var controller = BuildController(busLocationService.Object);

        var result = Assert.IsType<StatusCodeResult>(await controller.Search("istanbul", CancellationToken.None));

        Assert.Equal(StatusCodes.Status502BadGateway, result.StatusCode);
    }

    private static LocationsController BuildController(IBusLocationService busLocationService)
    {
        var appUserContext = new Mock<IAppUserContext>();
        appUserContext.SetupGet(c => c.AppUserId).Returns("user-1");

        return new LocationsController(busLocationService, appUserContext.Object, NullLogger<LocationsController>.Instance);
    }
}
