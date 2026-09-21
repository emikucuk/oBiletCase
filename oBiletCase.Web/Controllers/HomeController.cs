using System.Diagnostics;
using System.Globalization;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using oBiletCase.Application.Journeys;
using oBiletCase.Application.Locations;
using oBiletCase.Web;
using oBiletCase.Web.AppUser;
using oBiletCase.Web.Models;
using oBiletCase.Web.Models.Journeys;

namespace oBiletCase.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public sealed class HomeController(
    IValidator<JourneySearchCriteria> journeySearchValidator,
    IBusLocationService busLocationService,
    IAppUserContext appUserContext,
    IStringLocalizer<SharedResource> localizer,
    ILogger<HomeController> logger) : Controller
{

    private static readonly TimeSpan DefaultLocationsFetchTimeout = TimeSpan.FromSeconds(3);

    public async Task<IActionResult> Index(bool invalidSearch = false, CancellationToken cancellationToken = default)
    {
        var model = new JourneySearchViewModel
        {
            DepartureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
        };

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(DefaultLocationsFetchTimeout);

            var defaultLocations = await busLocationService.GetLocationsAsync(
                appUserContext.AppUserId, query: null, CultureInfo.CurrentUICulture.Name, timeoutCts.Token);

            if (defaultLocations.Count >= 2)
            {
                model.OriginId = defaultLocations[0].Id;
                model.OriginName = defaultLocations[0].Name;
                model.DestinationId = defaultLocations[1].Id;
                model.DestinationName = defaultLocations[1].Name;
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException or HttpRequestException or TaskCanceledException or OperationCanceledException)
        {
            logger.LogError(ex, "Varsayılan origin/destination için lokasyon listesi alınırken hata oluştu.");
        }

        ViewData["IsFreshSearch"] = true;
        ViewData["ShowInvalidSearchNotice"] = invalidSearch;

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(JourneySearchViewModel model)
    {
        if (model.OriginId is null)
        {
            ModelState.AddModelError(nameof(model.OriginId), localizer["ValidationOriginRequired"]);
        }

        if (model.DestinationId is null)
        {
            ModelState.AddModelError(nameof(model.DestinationId), localizer["ValidationDestinationRequired"]);
        }

        if (model.DepartureDate is null)
        {
            ModelState.AddModelError(nameof(model.DepartureDate), localizer["ValidationDateRequired"]);
        }

        if (model.OriginId is not null && model.DestinationId is not null && model.DepartureDate is not null)
        {
            var criteria = new JourneySearchCriteria(model.OriginId.Value, model.DestinationId.Value, model.DepartureDate.Value);
            var validationResult = journeySearchValidator.Validate(criteria);

            foreach (var error in validationResult.Errors)
            {
                var (field, message) = error.ErrorCode switch
                {
                    JourneySearchCriteriaValidator.PastDateErrorCode =>
                        (nameof(model.DepartureDate), (string)localizer["ValidationPastDate"]),
                    JourneySearchCriteriaValidator.SameLocationErrorCode =>
                        (nameof(model.DestinationId), (string)localizer["ValidationSameLocation"]),
                    _ => (string.Empty, error.ErrorMessage),
                };

                ModelState.AddModelError(field, message);
            }
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        return RedirectToAction(
            "Index",
            "Journeys",
            new
            {
                originId = model.OriginId,
                originName = model.OriginName,
                destinationId = model.DestinationId,
                destinationName = model.DestinationName,
                departureDate = model.DepartureDate!.Value.ToString("yyyy-MM-dd"),
            });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
