using System.Text.Json.Serialization;

namespace oBiletCase.Infrastructure.oBiletAPI.Contracts;

public sealed class DeviceSessionDto
{
    [JsonPropertyName("session-id")]
    public string SessionId { get; set; } = string.Empty;

    [JsonPropertyName("device-id")]
    public string DeviceId { get; set; } = string.Empty;
}
