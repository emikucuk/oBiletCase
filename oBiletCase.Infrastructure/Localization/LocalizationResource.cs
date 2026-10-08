namespace oBiletCase.Infrastructure.Localization;

/// <summary>
/// Arayüz metinlerinin bir dildeki karşılığı (eski <c>SharedResource.resx</c> kayıtları).
/// </summary>
internal sealed class LocalizationResource
{
    public LocalizationResource(string key, string culture, string value)
    {
        Key = key;
        Culture = culture;
        Value = value;
    }

    public string Key { get; private set; }

    public string Culture { get; private set; }

    public string Value { get; set; }
}
