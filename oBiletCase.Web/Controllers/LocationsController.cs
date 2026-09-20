using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using oBiletCase.Application.Locations;
using oBiletCase.Web.AppUser;
using oBiletCase.Web.Models.Locations;

namespace oBiletCase.Web.Controllers;

public sealed class LocationsController(IBusLocationService busLocationService, IAppUserContext appUserContext) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Search(string? query, CancellationToken cancellationToken)
    {
        var locations = await busLocationService.GetLocationsAsync(
            appUserContext.AppUserId, query, CultureInfo.CurrentUICulture.Name, cancellationToken);

        return Json(locations.Select(l => new LocationOptionViewModel(l.Id, l.Name)));
    }
}
