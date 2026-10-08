using System.Data.Common;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using oBiletCase.Infrastructure.Persistence;

namespace oBiletCase.Infrastructure.Localization;

/// <summary>
/// <c>LocalizationResources</c> tablosunun bellekteki kopyası. <see cref="Microsoft.Extensions.Localization.IStringLocalizer"/>
/// senkron olduğu için her metin için veritabanına gidilmez; tablo tek seferde okunup cache'lenir.
/// </summary>
internal sealed class LocalizationResourceCatalog
{
    private const string CacheKey = "Localization:Resources";

    // Tablodaki elle yapılan düzeltmelerin yeniden başlatma gerektirmeden yansıması için süreli.
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    // Veritabanına ulaşılamadığında sayfadaki her metin için yeniden bağlantı denenip sayfanın kilitlenmemesi için.
    private static readonly TimeSpan FailureCacheDuration = TimeSpan.FromMinutes(1);

    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LocalizationResourceCatalog> _logger;

    public LocalizationResourceCatalog(
        IDbContextFactory<AppDbContext> dbContextFactory, IMemoryCache cache, ILogger<LocalizationResourceCatalog> logger)
    {
        _dbContextFactory = dbContextFactory;
        _cache = cache;
        _logger = logger;
    }

    public string? Find(string key, CultureInfo culture)
    {
        var resources = GetResources();

        foreach (var candidate in CandidateCultures(culture))
        {
            if (resources.TryGetValue(candidate, out var values) && values.TryGetValue(key, out var value))
            {
                return value;
            }
        }

        return null;
    }

    public IReadOnlyDictionary<string, string> GetAll(CultureInfo culture, bool includeFallbackCulture)
    {
        var resources = GetResources();
        var cultures = includeFallbackCulture ? CandidateCultures(culture) : [LocalizationCultures.Normalize(culture)];
        var result = new Dictionary<string, string>();

        foreach (var candidate in cultures)
        {
            if (!resources.TryGetValue(candidate, out var values))
            {
                continue;
            }

            foreach (var (key, value) in values)
            {
                result.TryAdd(key, value);
            }
        }

        return result;
    }

    private static string[] CandidateCultures(CultureInfo culture)
    {
        var requested = LocalizationCultures.Normalize(culture);
        return requested == LocalizationCultures.Source ? [requested] : [requested, LocalizationCultures.Source];
    }

    private Dictionary<string, Dictionary<string, string>> GetResources()
    {
        if (_cache.TryGetValue(CacheKey, out Dictionary<string, Dictionary<string, string>>? cached) && cached is not null)
        {
            return cached;
        }

        Dictionary<string, Dictionary<string, string>> resources;
        TimeSpan duration;

        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            resources = dbContext.LocalizationResources
                .AsNoTracking()
                .AsEnumerable()
                .GroupBy(r => r.Culture)
                .ToDictionary(g => g.Key, g => g.ToDictionary(r => r.Key, r => r.Value));
            duration = CacheDuration;
        }
        catch (DbException ex)
        {
            _logger.LogError(ex, "Lokalizasyon kayıtları veritabanından okunamadı; metinler anahtar adıyla gösterilecek.");
            resources = [];
            duration = FailureCacheDuration;
        }

        _cache.Set(CacheKey, resources, duration);
        return resources;
    }
}
