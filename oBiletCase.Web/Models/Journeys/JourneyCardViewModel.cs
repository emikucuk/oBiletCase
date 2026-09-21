namespace oBiletCase.Web.Models.Journeys;

public sealed record JourneyCardViewModel(
    string PartnerName,
    string PartnerLogoUrl,
    string OriginStopName,
    string DestinationStopName,
    DateTime Departure,
    DateTime Arrival,
    TimeSpan Duration,
    string BusType,
    int AvailableSeats,
    decimal PriceAmount,
    string Currency,
    IReadOnlyList<JourneyFeatureViewModel> Features,
    JourneyDetailsViewModel Details,
    string ObiletUrl);
