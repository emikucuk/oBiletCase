using oBiletCase.Application.Journeys;

namespace oBiletCase.Tests.Application.Journeys;

public class JourneySearchCriteriaValidatorTests
{
    private readonly JourneySearchCriteriaValidator _validator = new();

    [Fact]
    public void Validate_gecerli_kriterlerde_hata_uretmez()
    {
        var criteria = new JourneySearchCriteria(1, 2, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));

        var result = _validator.Validate(criteria);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_bugunku_tarihte_hata_uretmez()
    {
        var criteria = new JourneySearchCriteria(1, 2, DateOnly.FromDateTime(DateTime.Today));

        var result = _validator.Validate(criteria);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_gecmis_tarihte_hata_uretir()
    {
        var criteria = new JourneySearchCriteria(1, 2, DateOnly.FromDateTime(DateTime.Today.AddDays(-1)));

        var result = _validator.Validate(criteria);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(JourneySearchCriteria.DepartureDate));
    }

    [Fact]
    public void Validate_ayni_kalkis_varis_lokasyonunda_hata_uretir()
    {
        var criteria = new JourneySearchCriteria(1, 1, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));

        var result = _validator.Validate(criteria);

        Assert.False(result.IsValid);
    }
}
