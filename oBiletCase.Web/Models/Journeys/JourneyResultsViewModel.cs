namespace oBiletCase.Web.Models.Journeys;

public sealed class JourneyResultsViewModel
{
    public required int OriginId { get; init; }

    public required string OriginName { get; init; }

    public required int DestinationId { get; init; }

    public required string DestinationName { get; init; }

    public required DateOnly DepartureDate { get; init; }

    public DateOnly? PreviousDate { get; init; }

    public required DateOnly NextDate { get; init; }

    public IReadOnlyList<JourneyCardViewModel> Journeys { get; init; } = [];

    public bool HasLoadError { get; init; }
}
