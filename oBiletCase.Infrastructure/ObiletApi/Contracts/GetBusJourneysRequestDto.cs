using System.Text.Json.Serialization;

namespace oBiletCase.Infrastructure.oBiletAPI.Contracts;

public sealed class GetBusJourneysRequestDto
{
    [JsonPropertyName("device-session")]
    public DeviceSessionDto DeviceSession { get; set; } = new();

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public JourneySearchDataDto Data { get; set; } = new();
}

public sealed class JourneySearchDataDto
{
    [JsonPropertyName("origin-id")]
    public int OriginId { get; set; }

    [JsonPropertyName("destination-id")]
    public int DestinationId { get; set; }

    [JsonPropertyName("departure-date")]
    public DateTime DepartureDate { get; set; }
}
