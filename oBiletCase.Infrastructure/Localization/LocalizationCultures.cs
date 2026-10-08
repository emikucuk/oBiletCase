using System.Globalization;

namespace oBiletCase.Infrastructure.Localization;

/// <summary>
/// Çeviri tablolarındaki <c>Culture</c> kolonunun değerleri. Kayıtlar bölgeden bağımsız iki harfli dil koduyla
/// tutulur; <c>tr-TR</c>/<c>en-US</c> gibi istek kültürleri bu değerlere indirgenerek eşlenir.
/// </summary>
internal static class LocalizationCultures
{
    public const string Turkish = "tr";
    public const string English = "en";

    /// <summary>
    /// Arayüzün varsayılan dili ve Obilet'in <c>language</c> parametresinden bağımsız olarak özellik adlarını
    /// döndürdüğü dil (2026-09-30'da canlı API'de tr-TR/en-EN karşılaştırmasıyla doğrulandı). Çeviri bulunamazsa buna düşülür.
    /// </summary>
    public const string Source = Turkish;

    public static string Normalize(CultureInfo culture) => culture.TwoLetterISOLanguageName;

    public static string Normalize(string cultureName) => Normalize(CultureInfo.GetCultureInfo(cultureName));
}
