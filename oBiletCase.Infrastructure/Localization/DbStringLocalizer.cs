using System.Globalization;
using Microsoft.Extensions.Localization;

namespace oBiletCase.Infrastructure.Localization;

/// <summary>
/// resx tabanlı <c>ResourceManagerStringLocalizer</c>'ın yerini alır; aynı sözleşmeyle (bulunamayan anahtar için
/// anahtarın kendisi + <c>ResourceNotFound</c>) çalıştığı için view/controller'lardaki kullanım değişmez.
/// </summary>
internal sealed class DbStringLocalizer : IStringLocalizer
{
    private readonly LocalizationResourceCatalog _catalog;

    public DbStringLocalizer(LocalizationResourceCatalog catalog)
    {
        _catalog = catalog;
    }

    public LocalizedString this[string name]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(name);

            var value = _catalog.Find(name, CultureInfo.CurrentUICulture);
            return new LocalizedString(name, value ?? name, resourceNotFound: value is null);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var format = this[name];
            var value = string.Format(CultureInfo.CurrentCulture, format.Value, arguments);
            return new LocalizedString(name, value, format.ResourceNotFound);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
        _catalog.GetAll(CultureInfo.CurrentUICulture, includeParentCultures)
            .Select(entry => new LocalizedString(entry.Key, entry.Value));
}
