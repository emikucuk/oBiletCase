using System.Text.Json.Serialization;

namespace oBiletCase.Infrastructure.oBiletAPI.Contracts;


public sealed class GetSessionRequestDto
{
    [JsonPropertyName("type")]
    public int Type { get; set; } = 1;

    [JsonPropertyName("connection")]
    public ConnectionInfoDto Connection { get; set; } = new();

    [JsonPropertyName("browser")]
    public BrowserInfoDto Browser { get; set; } = new();
}

public sealed class ConnectionInfoDto
{
    [JsonPropertyName("ip-address")]
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// API bu alanı boş/null string olarak kabul etmiyor. O Yüzden sabit bir değer gönderilir.
    /// </summary>
    [JsonPropertyName("port")]
    public string Port { get; set; } = "0";
}

public sealed class BrowserInfoDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "oBiletCase-Backend";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0.0";
}
