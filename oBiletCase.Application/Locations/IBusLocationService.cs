namespace oBiletCase.Application.Locations;

public interface IBusLocationService
{
    Task<IReadOnlyList<BusLocation>> GetLocationsAsync(string appUserId, string? query, string language, CancellationToken cancellationToken);
}
