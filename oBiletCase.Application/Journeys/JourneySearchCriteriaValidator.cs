using FluentValidation;

namespace oBiletCase.Application.Journeys;

public sealed class JourneySearchCriteriaValidator : AbstractValidator<JourneySearchCriteria>
{
    public const string PastDateErrorCode = "PastDate";
    public const string SameLocationErrorCode = "SameLocation";

    public JourneySearchCriteriaValidator()
    {
        RuleFor(x => x.DepartureDate)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.Today))
            .WithErrorCode(PastDateErrorCode)
            .WithMessage("DepartureDate geçmiş bir tarih olamaz.");

        RuleFor(x => x)
            .Must(x => x.OriginLocationId != x.DestinationLocationId)
            .WithErrorCode(SameLocationErrorCode)
            .WithMessage("OriginLocationId ve DestinationLocationId aynı olamaz.");
    }
}
