namespace oBiletCase.Application.Journeys;

/// <summary>
/// Result kartının expand edildiğinde gösterilen detaylar.
/// </summary>
public sealed record JourneyDetails(
    string? Description,
    int? CancellationOffsetHours,
    decimal? PartnerRating,
    bool MixedGendersAllowed,
    bool GovIdRequired);
