using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using oBiletCase.Application.Sessions;
using oBiletCase.Infrastructure.oBiletAPI.Contracts;

namespace oBiletCase.Infrastructure.oBiletAPI;

/// <summary>
/// <see cref="IObiletSessionAccessor"/>'ın Obilet API'siyle konuşan
/// implementasyonu.
/// </summary>
/// <remarks>
/// API dokümanı session'ın geçerlilik/yenilenme süresini belirtmiyor; bu
/// nedenle her <paramref name="appUserId"/> için edinilen session, makul
/// bir süreliğine (bkz. <see cref="CacheDuration"/>) bellekte tutulur ve
/// süre dolunca yeniden GetSession çağrılır. Bu süre gerçek API
/// davranışı netleştikçe (ör. session'ın ne zaman geçersiz olduğu
/// gözlemlenirse) güncellenmelidir. Session verisi kullanıcı bazında
/// (appUserId anahtarıyla) izole tutulur; kullanıcılar arasında
/// paylaşılmaz.
/// </remarks>
internal sealed class ObiletSessionAccessor : IObiletSessionAccessor
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);
    private const string CacheKeyPrefix = "oBiletAPI:Session:";

    private readonly ObiletApiClient _apiClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ObiletSessionAccessor> _logger;

    public ObiletSessionAccessor(ObiletApiClient apiClient, IMemoryCache cache, ILogger<ObiletSessionAccessor> logger)
    {
        _apiClient = apiClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ObiletSession> GetOrCreateSessionAsync(string appUserId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appUserId);

        var cacheKey = CacheKeyPrefix + appUserId;

        if (_cache.TryGetValue(cacheKey, out ObiletSession? cachedSession) && cachedSession is not null)
        {
            return cachedSession;
        }

        var envelope = await _apiClient.GetSessionAsync(cancellationToken);

        if (envelope is null || envelope.GetStatus() != ObiletResponseStatus.Success || envelope.Data is null)
        {
            // Obilet session'ı olmadan uygulamanın hiçbir çağrısı yapılamaz;
            // bu, beklenen bir iş kuralı ihlali değil, teknik bir arıza
            // niteliğindedir. Global Exception Handling eklendiğinde
            // (bkz. PROJECT_GUIDELINES.md §7) bu istisna orada yakalanıp
            // kullanıcıya güvenli bir hata sayfası olarak sunulmalıdır.
            _logger.LogError("Obilet session oluşturulamadı.");
            throw new InvalidOperationException("Obilet session oluşturulamadı.");
        }

        var session = new ObiletSession(envelope.Data.SessionId, envelope.Data.DeviceId);
        _cache.Set(cacheKey, session, CacheDuration);

        return session;
    }
}
