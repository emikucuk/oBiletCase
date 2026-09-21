using oBiletCase.Application.Journeys;
using oBiletCase.Application.Sessions;
using oBiletCase.Infrastructure.oBiletAPI.Contracts;

namespace oBiletCase.Infrastructure.oBiletAPI;

/// <summary>
/// ObiletApiClient servis testlerinin JSON ile yapılmadan; Moq ile stub'layarak iş mantığını test edebilmesi için var.
/// </summary>
internal interface IObiletApiClient
{
    Task<ObiletApiEnvelope<DeviceSessionDto>?> GetSessionAsync(CancellationToken cancellationToken);

    Task<ObiletApiEnvelope<List<BusLocationDto>>?> GetBusLocationsAsync(
        ObiletSession session, string? query, string language, CancellationToken cancellationToken);

    Task<ObiletApiEnvelope<List<JourneyDto>>?> GetBusJourneysAsync(
        ObiletSession session, JourneySearchCriteria criteria, string language, CancellationToken cancellationToken);
}
