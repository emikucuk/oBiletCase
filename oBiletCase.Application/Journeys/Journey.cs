namespace oBiletCase.Application.Journeys;

public sealed record Journey(
    long Id,
    int PartnerId,
    string PartnerName,
    string OriginStopName,
    string DestinationStopName,
    DateTime Departure,
    DateTime Arrival,
    TimeSpan Duration,
    string BusType,
    int AvailableSeats,
    decimal PriceAmount,
    string Currency,
    IReadOnlyList<JourneyFeature> Features,
    JourneyDetails Details);
