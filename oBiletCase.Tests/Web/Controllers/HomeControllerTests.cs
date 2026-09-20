using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using oBiletCase.Application.Journeys;
using oBiletCase.Web;
using oBiletCase.Web.Controllers;
using oBiletCase.Web.Models.Journeys;

namespace oBiletCase.Tests.Web.Controllers;

public class HomeControllerTests
{
    private readonly HomeController _controller = new(
        new JourneySearchCriteriaValidator(), BuildLocalizer());

    [Fact]
    public void Index_get_varsayilan_tarih_yarindir()
    {
        var result = Assert.IsType<ViewResult>(_controller.Index());
        var model = Assert.IsType<JourneySearchViewModel>(result.Model);

        Assert.Equal(DateOnly.FromDateTime(DateTime.Today.AddDays(1)), model.DepartureDate);
    }

    [Fact]
    public void Index_post_ayni_lokasyonda_model_state_hatasiyla_view_doner()
    {
        var model = new JourneySearchViewModel
        {
            OriginId = 1,
            OriginName = "A",
            DestinationId = 1,
            DestinationName = "A",
            DepartureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
        };

        var result = Assert.IsType<ViewResult>(_controller.Index(model));

        Assert.False(_controller.ModelState.IsValid);
        Assert.Same(model, result.Model);
    }

    [Fact]
    public void Index_post_gecmis_tarihte_model_state_hatasiyla_view_doner()
    {
        var model = new JourneySearchViewModel
        {
            OriginId = 1,
            OriginName = "A",
            DestinationId = 2,
            DestinationName = "B",
            DepartureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
        };

        _controller.Index(model);

        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public void Index_post_eksik_lokasyonda_model_state_hatasiyla_view_doner()
    {
        var model = new JourneySearchViewModel
        {
            DepartureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
        };

        _controller.Index(model);

        Assert.False(_controller.ModelState.IsValid);
        Assert.True(_controller.ModelState.ContainsKey(nameof(JourneySearchViewModel.OriginId)));
        Assert.True(_controller.ModelState.ContainsKey(nameof(JourneySearchViewModel.DestinationId)));
    }

    [Fact]
    public void Index_post_gecerli_kriterlerde_journeys_index_e_yonlendirir()
    {
        var model = new JourneySearchViewModel
        {
            OriginId = 349,
            OriginName = "İstanbul Avrupa",
            DestinationId = 356,
            DestinationName = "Ankara",
            DepartureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
        };

        var result = Assert.IsType<RedirectToActionResult>(_controller.Index(model));

        Assert.Equal("Index", result.ActionName);
        Assert.Equal("Journeys", result.ControllerName);
    }

    private static IStringLocalizer<SharedResource> BuildLocalizer()
    {
        var mock = new Mock<IStringLocalizer<SharedResource>>();
        mock.Setup(l => l[It.IsAny<string>()]).Returns((string key) => new LocalizedString(key, key));
        return mock.Object;
    }
}
