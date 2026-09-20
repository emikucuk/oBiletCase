using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using oBiletCase.Infrastructure.oBiletAPI.Contracts;

namespace oBiletCase.Infrastructure.oBiletAPI;

/// <summary>
/// obilet.com Business API'sine ham HTTP çağrılarını yapan istemci.
/// Yalnızca Infrastructure içinde tüketilir; Application bu sınıfı bilmez,
/// yalnızca ilgili feature'ların (ör. Sessions) tanımladığı sözleşmeleri
/// görür. İstek/yanıt içerikleri (session-id, device-id, token) asla
/// loglanmaz; yalnızca HTTP durum kodu ve status alanı loglanır.
/// </summary>
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
}
