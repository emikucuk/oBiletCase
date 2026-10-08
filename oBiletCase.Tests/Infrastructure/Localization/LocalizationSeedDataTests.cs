using oBiletCase.Infrastructure.Localization;

namespace oBiletCase.Tests.Infrastructure.Localization;

public class LocalizationSeedDataTests
{
    [Fact]
    public void Resources_her_anahtar_iki_dilde_de_bulunur()
    {
        var turkishKeys = KeysOf(LocalizationCultures.Turkish);
        var englishKeys = KeysOf(LocalizationCultures.English);

        Assert.Empty(turkishKeys.Except(englishKeys));
        Assert.Empty(englishKeys.Except(turkishKeys));
    }

    [Fact]
    public void FeatureTranslations_her_ozellik_iki_dilde_de_bulunur()
    {
        var turkishIds = LocalizationSeedData.FeatureTranslations.Where(t => t.Culture == LocalizationCultures.Turkish).Select(t => t.FeatureId);
        var englishIds = LocalizationSeedData.FeatureTranslations.Where(t => t.Culture == LocalizationCultures.English).Select(t => t.FeatureId);

        Assert.Equal(turkishIds.Order(), englishIds.Order());
    }

    private static HashSet<string> KeysOf(string culture) =>
        LocalizationSeedData.Resources.Where(r => r.Culture == culture).Select(r => r.Key).ToHashSet();
}
