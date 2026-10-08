namespace oBiletCase.Infrastructure.Localization;

/// <summary>
/// Obilet'in sefer yanıtındaki bir özelliğin (<c>features[].id</c>) bir dildeki adı.
/// </summary>
internal sealed class FeatureTranslation
{
    public FeatureTranslation(int featureId, string culture, string name)
    {
        FeatureId = featureId;
        Culture = culture;
        Name = name;
    }

    public int FeatureId { get; private set; }

    public string Culture { get; private set; }

    public string Name { get; set; }
}
