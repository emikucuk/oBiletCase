using oBiletCase.Infrastructure.oBiletAPI.Contracts;

namespace oBiletCase.Infrastructure.Localization;

internal interface IFeatureTranslationStore
{
    /// <summary>
    /// Verilen özelliklerin <paramref name="language"/> dilindeki adlarını özellik id'sine göre döner. Çevirisi
    /// olmayan özellik için API'den gelen ad kullanılır.
    /// </summary>
    Task<IReadOnlyDictionary<int, string>> GetNamesAsync(
        IReadOnlyCollection<JourneyFeatureDto> features, string language, CancellationToken cancellationToken);
}
