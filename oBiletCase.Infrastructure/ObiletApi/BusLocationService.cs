using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using oBiletCase.Application.Locations;
using oBiletCase.Application.Sessions;
using oBiletCase.Infrastructure.oBiletAPI.Contracts;

namespace oBiletCase.Infrastructure.oBiletAPI;

internal sealed class BusLocationService : IBusLocationService
{

    private static readonly TimeSpan DefaultLocationsCacheDuration = TimeSpan.FromMinutes(60);
    private const string DefaultLocationsCacheKeyPrefix = "oBiletAPI:Locations:Default:";

    private readonly IObiletApiClient _apiClient;
    private readonly IObiletSessionAccessor _sessionAccessor;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BusLocationService> _logger;

    public BusLocationService(
        IObiletApiClient apiClient,
        IObiletSessionAccessor sessionAccessor,
        IMemoryCache cache,
        ILogger<BusLocationService> logger)
    {
        _apiClient = apiClient;
        _sessionAccessor = sessionAccessor;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IReadOnlyList<BusLocation>> GetLocationsAsync(
        string appUserId, string? query, string language, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appUserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(language);

        var isDefaultListRequest = string.IsNullOrWhiteSpace(query);
        var cacheKey = DefaultLocationsCacheKeyPrefix + language;

        if (isDefaultListRequest && _cache.TryGetValue(cacheKey, out IReadOnlyList<BusLocation>? cachedLocations)
            && cachedLocations is not null)
        {
            return cachedLocations;
        }

        var session = await _sessionAccessor.GetOrCreateSessionAsync(appUserId, cancellationToken);

        var envelope = await _apiClient.GetBusLocationsAsync(
            session, query, ObiletLanguage.ToObiletWireValue(language), cancellationToken);

        if (envelope is null || envelope.GetStatus() != ObiletResponseStatus.Success || envelope.Data is null)
        {
            _logger.LogError("Obilet bus location listesi alınamadı.");
            throw new InvalidOperationException("Obilet bus location listesi alınamadı.");
        }

        var locations = envelope.Data
            .Select(dto => new BusLocation(dto.Id, dto.Name))
            .ToList();

        if (isDefaultListRequest)
        {
            _cache.Set(cacheKey, (IReadOnlyList<BusLocation>)locations, DefaultLocationsCacheDuration);
        }

        return locations;
    }
}
