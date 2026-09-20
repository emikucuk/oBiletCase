using System.Text.Json.Serialization;

namespace oBiletCase.Infrastructure.oBiletAPI.Contracts;

public sealed class GetBusLocationsRequestDto
{
    [JsonPropertyName("data")]
    public string? Data { get; set; }

    [JsonPropertyName("device-session")]
    public DeviceSessionDto DeviceSession { get; set; } = new();

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;
}
