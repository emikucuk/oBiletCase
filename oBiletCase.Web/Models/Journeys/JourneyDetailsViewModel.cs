namespace oBiletCase.Web.Models.Journeys;

public sealed record JourneyDetailsViewModel(
    string? Description,
    int? CancellationOffsetHours,
    decimal? PartnerRating,
    bool MixedGendersAllowed,
    bool GovIdRequired);
