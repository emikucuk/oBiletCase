namespace oBiletCase.Application.Journeys;

public sealed record JourneySearchCriteria(int OriginLocationId, int DestinationLocationId, DateOnly DepartureDate);
