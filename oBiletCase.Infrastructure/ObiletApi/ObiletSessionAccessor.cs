using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using oBiletCase.Application.Sessions;
using oBiletCase.Infrastructure.oBiletAPI.Contracts;

namespace oBiletCase.Infrastructure.oBiletAPI;

internal sealed class ObiletSessionAccessor : IObiletSessionAccessor
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);
    private const string CacheKeyPrefix = "oBiletAPI:Session:";

    private readonly IObiletApiClient _apiClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ObiletSessionAccessor> _logger;

    public ObiletSessionAccessor(IObiletApiClient apiClient, IMemoryCache cache, ILogger<ObiletSessionAccessor> logger)
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
            _logger.LogError("Obilet session oluşturulamadı.");
            throw new InvalidOperationException("Obilet session oluşturulamadı.");
        }

        var session = new ObiletSession(envelope.Data.SessionId, envelope.Data.DeviceId);
        _cache.Set(cacheKey, session, CacheDuration);

        return session;
    }
}
