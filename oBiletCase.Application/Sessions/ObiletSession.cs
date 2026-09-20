namespace oBiletCase.Application.Sessions;

/// <summary>
/// Application'ın kullandığı Obilet session temsili. Obilet API'sinin JSON
/// alan adlarını veya HTTP ayrıntılarını içermez; bunlar Infrastructure
/// sınırında kalır.
/// </summary>
public sealed record ObiletSession(string SessionId, string DeviceId);
