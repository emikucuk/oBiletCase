using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using oBiletCase.Application.Journeys;
using oBiletCase.Application.Sessions;
using oBiletCase.Infrastructure.oBiletAPI.Contracts;

namespace oBiletCase.Infrastructure.oBiletAPI;


internal sealed class ObiletApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly oBiletAPIOptions _options;
    private readonly ILogger<ObiletApiClient> _logger;

    public ObiletApiClient(
        IHttpClientFactory httpClientFactory,
        IOptions<oBiletAPIOptions> options,
        ILogger<ObiletApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ObiletApiEnvelope<DeviceSessionDto>?> GetSessionAsync(CancellationToken cancellationToken)
    {
        var request = new GetSessionRequestDto
        {
            Connection =
            {
                IpAddress = string.IsNullOrWhiteSpace(_options.OutboundIpAddress)
                    ? oBiletAPIOptions.DefaultOutboundIpAddress
                    : _options.OutboundIpAddress,
            },
        };

        var client = _httpClientFactory.CreateClient(oBiletAPIOptions.HttpClientName);

        using var response = await client.PostAsJsonAsync("client/getsession", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Obilet GetSession isteği HTTP {StatusCode} ile başarısız oldu.",
                (int)response.StatusCode);
            return null;
        }

        var envelope = await response.Content.ReadFromJsonAsync<ObiletApiEnvelope<DeviceSessionDto>>(cancellationToken);

        if (envelope is not null && envelope.GetStatus() != ObiletResponseStatus.Success)
        {
            _logger.LogWarning("Obilet GetSession isteği status={Status} döndürdü.", envelope.Status);
        }

        return envelope;
    }

    public async Task<ObiletApiEnvelope<List<BusLocationDto>>?> GetBusLocationsAsync(
        ObiletSession session, string? query, string language, CancellationToken cancellationToken)
    {
        var request = new GetBusLocationsRequestDto
        {
            Data = query,
            DeviceSession = new DeviceSessionDto
            {
                SessionId = session.SessionId,
                DeviceId = session.DeviceId,
            },
            Date = DateTime.Now,
            Language = language,
        };

        var client = _httpClientFactory.CreateClient(oBiletAPIOptions.HttpClientName);

        using var response = await client.PostAsJsonAsync("location/getbuslocations", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Obilet GetBusLocations isteği HTTP {StatusCode} ile başarısız oldu.",
                (int)response.StatusCode);
            return null;
        }

        var envelope = await response.Content.ReadFromJsonAsync<ObiletApiEnvelope<List<BusLocationDto>>>(cancellationToken);

        if (envelope is not null && envelope.GetStatus() != ObiletResponseStatus.Success)
        {
            _logger.LogWarning("Obilet GetBusLocations isteği status={Status} döndürdü.", envelope.Status);
        }

        return envelope;
    }

    public async Task<ObiletApiEnvelope<List<JourneyDto>>?> GetBusJourneysAsync(
        ObiletSession session, JourneySearchCriteria criteria, string language, CancellationToken cancellationToken)
    {
        var request = new GetBusJourneysRequestDto
        {
            DeviceSession = new DeviceSessionDto
            {
                SessionId = session.SessionId,
                DeviceId = session.DeviceId,
            },
            Date = DateTime.Now,
            Language = language,
            Data = new JourneySearchDataDto
            {
                OriginId = criteria.OriginLocationId,
                DestinationId = criteria.DestinationLocationId,
                DepartureDate = criteria.DepartureDate.ToDateTime(TimeOnly.MinValue),
            },
        };

        var client = _httpClientFactory.CreateClient(oBiletAPIOptions.HttpClientName);

        using var response = await client.PostAsJsonAsync("journey/getbusjourneys", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Obilet GetBusJourneys isteği HTTP {StatusCode} ile başarısız oldu.",
                (int)response.StatusCode);
            return null;
        }

        var envelope = await response.Content.ReadFromJsonAsync<ObiletApiEnvelope<List<JourneyDto>>>(cancellationToken);

        if (envelope is not null && envelope.GetStatus() != ObiletResponseStatus.Success)
        {
            _logger.LogWarning("Obilet GetBusJourneys isteği status={Status} döndürdü.", envelope.Status);
        }

        return envelope;
    }
}
