using System.Text.Json.Serialization;

namespace oBiletCase.Infrastructure.oBiletAPI.Contracts;

/// <summary>
/// Obilet API'sinin GetSession yanıtında döndürdüğü ve sonraki her
/// istekte "device-session" alanı olarak geri gönderilmesi gereken
/// session/device kimlik çifti.
/// </summary>
public sealed class DeviceSessionDto
{
    [JsonPropertyName("session-id")]
    public string SessionId { get; set; } = string.Empty;

    [JsonPropertyName("device-id")]
    public string DeviceId { get; set; } = string.Empty;
}
