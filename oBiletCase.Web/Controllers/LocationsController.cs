using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using oBiletCase.Application.Locations;
using oBiletCase.Web.AppUser;
using oBiletCase.Web.Models.Locations;

namespace oBiletCase.Web.Controllers;

public sealed class LocationsController(
    IBusLocationService busLocationService,
    IAppUserContext appUserContext,
    ILogger<LocationsController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Search(string? query, CancellationToken cancellationToken)
    {
        try
        {
            var locations = await busLocationService.GetLocationsAsync(
                appUserContext.AppUserId, query, CultureInfo.CurrentUICulture.Name, cancellationToken);

            return Json(locations.Select(l => new LocationOptionViewModel(l.Id, l.Name)));
        }
        catch (Exception ex) when (ex is InvalidOperationException or HttpRequestException or TaskCanceledException)
        {
            logger.LogError(ex, "Lokasyon araması alınırken hata oluştu.");
            return StatusCode(StatusCodes.Status502BadGateway);
        }
    }
}
