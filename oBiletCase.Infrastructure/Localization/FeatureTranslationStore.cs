using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using oBiletCase.Infrastructure.oBiletAPI.Contracts;
using oBiletCase.Infrastructure.Persistence;

namespace oBiletCase.Infrastructure.Localization;

/// <summary>
/// Obilet özellik adlarını <c>FeatureTranslations</c> tablosundan çevirir. API adları her dilde Türkçe döndürdüğü
/// için Türkçe'de API'nin adı olduğu gibi kullanılır; tabloda Türkçe kaydı olmayan yeni bir özellik görüldüğünde
/// Türkçe adıyla tabloya eklenir ki diğer dillerdeki karşılığı sonradan doldurulabilsin.
/// </summary>
internal sealed class FeatureTranslationStore : IFeatureTranslationStore
{
    private const string CacheKey = "Localization:FeatureTranslations";

    // Tabloya elle girilen yeni çevirilerin yeniden başlatma gerektirmeden yansıması için süreli.
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<FeatureTranslationStore> _logger;

    public FeatureTranslationStore(
        IDbContextFactory<AppDbContext> dbContextFactory, IMemoryCache cache, ILogger<FeatureTranslationStore> logger)
    {
        _dbContextFactory = dbContextFactory;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IReadOnlyDictionary<int, string>> GetNamesAsync(
        IReadOnlyCollection<JourneyFeatureDto> features, string language, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(language);

        var distinctFeatures = features.DistinctBy(f => f.Id).ToList();
        if (distinctFeatures.Count == 0)
        {
            return new Dictionary<int, string>();
        }

        Dictionary<(int FeatureId, string Culture), string> translations;
        try
        {
            translations = await GetTranslationsAsync(cancellationToken);
            await RegisterMissingAsync(distinctFeatures, translations, cancellationToken);
        }
        catch (DbException ex)
        {
            _logger.LogWarning(ex, "Özellik çevirileri veritabanından okunamadı; API'den gelen adlar kullanılacak.");
            return distinctFeatures.ToDictionary(f => f.Id, f => f.Name);
        }

        var culture = LocalizationCultures.Normalize(language);
        if (culture == LocalizationCultures.Source)
        {
            return distinctFeatures.ToDictionary(f => f.Id, f => f.Name);
        }

        return distinctFeatures.ToDictionary(
            f => f.Id,
            f => translations.TryGetValue((f.Id, culture), out var name) ? name : f.Name);
    }

    private async Task<Dictionary<(int FeatureId, string Culture), string>> GetTranslationsAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CacheKey, out Dictionary<(int, string), string>? cached) && cached is not null)
        {
            return cached;
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var translations = await dbContext.FeatureTranslations
            .AsNoTracking()
            .ToDictionaryAsync(t => (t.FeatureId, t.Culture), t => t.Name, cancellationToken);

        _cache.Set(CacheKey, translations, CacheDuration);
        return translations;
    }

    private async Task RegisterMissingAsync(
        IReadOnlyCollection<JourneyFeatureDto> features,
        IReadOnlyDictionary<(int FeatureId, string Culture), string> translations,
        CancellationToken cancellationToken)
    {
        var missing = features
            .Where(f => !translations.ContainsKey((f.Id, LocalizationCultures.Source)))
            .ToList();

        if (missing.Count == 0)
        {
            return;
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.FeatureTranslations.AddRange(
            missing.Select(f => new FeatureTranslation(f.Id, LocalizationCultures.Source, f.Name)));

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "{Count} yeni özellik çeviri tablosuna eklendi: {FeatureIds}", missing.Count, missing.Select(f => f.Id));
        }
        catch (DbUpdateException ex)
        {
            // Eşzamanlı iki arama aynı özelliği eklemeye çalışırsa birincil anahtar çakışır; kayıt zaten eklenmiştir.
            _logger.LogWarning(ex, "Yeni özellikler çeviri tablosuna eklenemedi; bir sonraki aramada yeniden denenecek.");
        }

        _cache.Remove(CacheKey);
    }
}
