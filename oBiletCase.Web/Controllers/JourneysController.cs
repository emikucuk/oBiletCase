using System.Globalization;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using oBiletCase.Application.Journeys;
using oBiletCase.Web.AppUser;
using oBiletCase.Web.Models.Journeys;

namespace oBiletCase.Web.Controllers;

public sealed class JourneysController(
    IBusJourneyService busJourneyService,
    IValidator<JourneySearchCriteria> journeySearchValidator,
    IAppUserContext appUserContext,
    ILogger<JourneysController> logger) : Controller
{
    private const string ObiletJourneyUrlTemplate = "https://www.obilet.com/seferler/{0}-{1}/{2:yyyy-MM-dd}/{3}";

    private const string PartnerLogoUrlTemplate = "https://s3.eu-central-1.amazonaws.com/static.obilet.com/images/partner/{0}-sm.png";
    private const string FeatureIconUrlTemplate = "https://s3.eu-central-1.amazonaws.com/static.obilet.com/images/feature/{0}.svg";

    public async Task<IActionResult> Index(
        int originId,
        string originName,
        int destinationId,
        string destinationName,
        DateOnly departureDate,
        CancellationToken cancellationToken = default)
    {
        var criteria = new JourneySearchCriteria(originId, destinationId, departureDate);

        if (!journeySearchValidator.Validate(criteria).IsValid)
        {
            return RedirectToAction("Index", "Home", new { invalidSearch = true });
        }

        IReadOnlyList<Journey> journeys = [];
        var hasLoadError = false;

        try
        {
            journeys = await busJourneyService.GetJourneysAsync(
                appUserContext.AppUserId, criteria, CultureInfo.CurrentUICulture.Name, cancellationToken);
        }
        catch (Exception ex) when (ex is InvalidOperationException or HttpRequestException or TaskCanceledException)
        {
            logger.LogError(ex, "Sefer listesi alınırken hata oluştu.");
            hasLoadError = true;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var previousDate = departureDate > today ? departureDate.AddDays(-1) : (DateOnly?)null;

        var model = new JourneyResultsViewModel
        {
            OriginId = originId,
            OriginName = originName,
            DestinationId = destinationId,
            DestinationName = destinationName,
            DepartureDate = departureDate,
            PreviousDate = previousDate,
            NextDate = departureDate.AddDays(1),
            HasLoadError = hasLoadError,
            Journeys = journeys
                .Select(journey => new JourneyCardViewModel(
                    journey.PartnerName,
                    string.Format(CultureInfo.InvariantCulture, PartnerLogoUrlTemplate, journey.PartnerId),
                    journey.OriginStopName,
                    journey.DestinationStopName,
                    journey.Departure,
                    journey.Arrival,
                    journey.Duration,
                    journey.BusType,
                    journey.AvailableSeats,
                    journey.PriceAmount,
                    journey.Currency,
                    journey.Features
                        .Select(feature => new JourneyFeatureViewModel(
                            feature.Name,
                            string.Format(CultureInfo.InvariantCulture, FeatureIconUrlTemplate, feature.Id)))
                        .ToList(),
                    new JourneyDetailsViewModel(
                        journey.Details.Description,
                        journey.Details.CancellationOffsetHours,
                        journey.Details.PartnerRating,
                        journey.Details.MixedGendersAllowed,
                        journey.Details.GovIdRequired),
                    string.Format(
                        CultureInfo.InvariantCulture,
                        ObiletJourneyUrlTemplate,
                        originId,
                        destinationId,
                        departureDate.ToDateTime(TimeOnly.MinValue),
                        journey.Id)))
                .ToList(),
        };

        return View(model);
    }
}
