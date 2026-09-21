using System.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using oBiletCase.Application.Journeys;
using oBiletCase.Web;
using oBiletCase.Web.Models;
using oBiletCase.Web.Models.Journeys;

namespace oBiletCase.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public sealed class HomeController(
    IValidator<JourneySearchCriteria> journeySearchValidator,
    IStringLocalizer<SharedResource> localizer) : Controller
{
    public IActionResult Index(bool invalidSearch = false)
    {
        var model = new JourneySearchViewModel
        {
            DepartureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
        };

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
