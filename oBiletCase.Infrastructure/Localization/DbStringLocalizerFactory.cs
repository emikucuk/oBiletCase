using Microsoft.Extensions.Localization;

namespace oBiletCase.Infrastructure.Localization;

/// <summary>
/// resx'teki gibi kaynak tipi başına ayrı dosya yoktur; tüm uygulama tek bir anahtar alanını paylaşır. Bu yüzden
/// hangi tip/baseName istenirse istensin (<c>IStringLocalizer&lt;SharedResource&gt;</c>, DataAnnotations vb.) aynı localizer döner.
/// </summary>
internal sealed class DbStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly DbStringLocalizer _localizer;

    public DbStringLocalizerFactory(LocalizationResourceCatalog catalog)
    {
        _localizer = new DbStringLocalizer(catalog);
    }

    public IStringLocalizer Create(Type resourceSource) => _localizer;

    public IStringLocalizer Create(string baseName, string location) => _localizer;
}
