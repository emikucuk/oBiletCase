using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Http;

namespace oBiletCase.Infrastructure.oBiletAPI;

/// <summary>
/// API bağlantısı için gereken yapılandırma değerleri.
/// appsettings.json içindeki "oBiletAPI" bölümünden bağlanır.
/// </summary>
public sealed class oBiletAPIOptions
{
    public const string SectionName = "oBiletAPI";

    public const string HttpClientName = "oBiletAPI";

    [Required]
    [Url]
    public string BaseUrl { get; set; } = string.Empty;

    [Required]
    public string ApiClientToken { get; set; } = string.Empty;

    [Range(1, 120)]
    public int TimeoutSeconds { get; set; } = 30;

    public string? OutboundIpAddress { get; set; }

    public const string DefaultOutboundIpAddress = "127.0.0.1";
}
