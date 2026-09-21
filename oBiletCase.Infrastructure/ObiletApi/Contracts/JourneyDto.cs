using System.Text.Json.Serialization;

namespace oBiletCase.Infrastructure.oBiletAPI.Contracts;

/// <summary>
/// Dokümanda verilen alanlar ve mantıksal birkaç alan daha alındı. Tüm alanlar alınıp, AutoMapper kullanılabilir
///  ama karmaşıklığı azaltmak için sadece gerekli alanlar alındı.
/// </summary>
public sealed class JourneyDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("partner-id")]
    public int PartnerId { get; set; }

    [JsonPropertyName("partner-name")]
    public string PartnerName { get; set; } = string.Empty;

    [JsonPropertyName("bus-type")]
    public string BusType { get; set; } = string.Empty;

    [JsonPropertyName("available-seats")]
    public int AvailableSeats { get; set; }

    [JsonPropertyName("cancellation-offset")]
    public int? CancellationOffsetHours { get; set; }

    [JsonPropertyName("partner-rating")]
    public decimal? PartnerRating { get; set; }

    [JsonPropertyName("journey")]
    public JourneyDetailDto Journey { get; set; } = new();

    [JsonPropertyName("features")]
    public List<JourneyFeatureDto> Features { get; set; } = [];
}

public sealed class JourneyFeatureDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public sealed class JourneyDetailDto
{
    [JsonPropertyName("origin")]
    public string Origin { get; set; } = string.Empty;

    [JsonPropertyName("destination")]
    public string Destination { get; set; } = string.Empty;

    [JsonPropertyName("departure")]
    public DateTime Departure { get; set; }

    [JsonPropertyName("arrival")]
    public DateTime Arrival { get; set; }

    [JsonPropertyName("duration")]
    public TimeSpan Duration { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("internet-price")]
    public decimal InternetPrice { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("policy")]
    public JourneyPolicyDto Policy { get; set; } = new();
}

public sealed class JourneyPolicyDto
{
    [JsonPropertyName("mixed-genders")]
    public bool MixedGenders { get; set; }

    [JsonPropertyName("gov-id")]
    public bool GovIdRequired { get; set; }
}
