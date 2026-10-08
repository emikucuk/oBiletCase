using Microsoft.Extensions.Logging;
using oBiletCase.Application.Journeys;
using oBiletCase.Application.Sessions;
using oBiletCase.Infrastructure.Localization;
using oBiletCase.Infrastructure.oBiletAPI.Contracts;

namespace oBiletCase.Infrastructure.oBiletAPI;

internal sealed class BusJourneyService : IBusJourneyService
{
    private readonly IObiletApiClient _apiClient;
    private readonly IObiletSessionAccessor _sessionAccessor;
    private readonly IFeatureTranslationStore _featureTranslations;
    private readonly ILogger<BusJourneyService> _logger;

    public BusJourneyService(
        IObiletApiClient apiClient,
        IObiletSessionAccessor sessionAccessor,
        IFeatureTranslationStore featureTranslations,
        ILogger<BusJourneyService> logger)
    {
        _apiClient = apiClient;
        _sessionAccessor = sessionAccessor;
        _featureTranslations = featureTranslations;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Journey>> GetJourneysAsync(
        string appUserId, JourneySearchCriteria criteria, string language, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appUserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(language);

        var session = await _sessionAccessor.GetOrCreateSessionAsync(appUserId, cancellationToken);

        var envelope = await _apiClient.GetBusJourneysAsync(
            session, criteria, ObiletLanguage.ToObiletWireValue(language), cancellationToken);

        if (envelope is null || envelope.GetStatus() != ObiletResponseStatus.Success || envelope.Data is null)
        {
            _logger.LogError("Obilet sefer listesi alınamadı.");
            throw new InvalidOperationException("Obilet sefer listesi alınamadı.");
        }

        var featureNames = await _featureTranslations.GetNamesAsync(
            envelope.Data.SelectMany(dto => dto.Features).ToList(), language, cancellationToken);

        return envelope.Data
            .Select(dto => new Journey(
                dto.Id,
                dto.PartnerId,
                dto.PartnerName,
                dto.Journey.Origin,
                dto.Journey.Destination,
                dto.Journey.Departure,
                dto.Journey.Arrival,
                dto.Journey.Duration,
                dto.BusType,
                dto.AvailableSeats,
                dto.Journey.InternetPrice,
                dto.Journey.Currency,
                dto.Features.Select(f => new JourneyFeature(f.Id, featureNames.GetValueOrDefault(f.Id, f.Name))).ToList(),
                new JourneyDetails(
                    string.IsNullOrWhiteSpace(dto.Journey.Description) ? null : dto.Journey.Description,
                    dto.CancellationOffsetHours,
                    dto.PartnerRating,
                    dto.Journey.Policy.MixedGenders,
                    dto.Journey.Policy.GovIdRequired)))
            .OrderBy(journey => journey.Departure)
            .ToList();
    }
}
