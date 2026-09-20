using System.Text.Json.Serialization;

namespace oBiletCase.Infrastructure.oBiletAPI.Contracts;

/// <summary>
/// API'sinin tüm endpoint'lerinde ortak olan yanıt modeli. 
/// Dokümanda yer almayan ekstra alanlar ("correlation-id" vb.) kasıtlı olarak modellenmemiştir
/// </summary>
public sealed class ObiletApiEnvelope<TData>
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public TData? Data { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("user-message")]
    public string? UserMessage { get; set; }

    [JsonPropertyName("api-request-id")]
    public string? ApiRequestId { get; set; }

    [JsonPropertyName("controller")]
    public string? Controller { get; set; }

    public ObiletResponseStatus? GetStatus() => Status switch
    {
        "Success" => ObiletResponseStatus.Success,
        "InvalidDepartureDate" => ObiletResponseStatus.InvalidDepartureDate,
        "InvalidRoute" => ObiletResponseStatus.InvalidRoute,
        "InvalidLocation" => ObiletResponseStatus.InvalidLocation,
        "Timeout" => ObiletResponseStatus.Timeout,
        _ => null,
    };
}
