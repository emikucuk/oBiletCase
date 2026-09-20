using System.Text.Json.Serialization;

namespace oBiletCase.Infrastructure.oBiletAPI.Contracts;

/// <summary>
/// obilet.com API'sinin tüm endpoint'lerinde ortak olan yanıt zarfı.
/// Alan adları docs/obilet.com-API-Integration-Specs.pdf ve gerçek API
/// yanıtlarıyla doğrulanmıştır. Dokümanda yer almayan ekstra alanlar
/// (ör. "correlation-id") kasıtlı olarak modellenmemiştir; System.Text.Json
/// bilinmeyen alanları sessizce yok sayar.
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

    /// <summary>
    /// <see cref="Status"/> alanını Appendix A'da listelenen bilinen
    /// değerlere eşler. Listede olmayan bir değer gelirse (API'nin
    /// belgelenmemiş bir durumu döndürmesi ihtimaline karşı)
    /// <see cref="ObiletResponseStatus.Unknown"/> döner; bu durumda
    /// çağıran taraf ham <see cref="Message"/>/<see cref="UserMessage"/>
    /// alanlarına bakmalıdır.
    /// </summary>
    public ObiletResponseStatus GetStatus() => Status switch
    {
        "Success" => ObiletResponseStatus.Success,
        "InvalidDepartureDate" => ObiletResponseStatus.InvalidDepartureDate,
        "InvalidRoute" => ObiletResponseStatus.InvalidRoute,
        "InvalidLocation" => ObiletResponseStatus.InvalidLocation,
        "Timeout" => ObiletResponseStatus.Timeout,
        _ => ObiletResponseStatus.Unknown,
    };
}
