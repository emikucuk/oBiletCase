namespace oBiletCase.Web.Models.Journeys;

public sealed class JourneySearchViewModel
{
    public int? OriginId { get; set; }

    public string? OriginName { get; set; }

    public int? DestinationId { get; set; }

    public string? DestinationName { get; set; }

    public DateOnly? DepartureDate { get; set; }
}
