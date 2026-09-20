using Microsoft.AspNetCore.Mvc;
using oBiletCase.Web.Models.Journeys;

namespace oBiletCase.Web.Controllers;

public sealed class JourneysController : Controller
{
    public IActionResult Index(int originId, string originName, int destinationId, string destinationName, DateOnly departureDate)
    {
        var model = new JourneySearchSummaryViewModel(originName, destinationName, departureDate);

        return View(model);
    }
}
