namespace oBiletCase.Application.Journeys;

public interface IBusJourneyService
{
    Task<IReadOnlyList<Journey>> GetJourneysAsync(
        string appUserId, JourneySearchCriteria criteria, string language, CancellationToken cancellationToken);
}
