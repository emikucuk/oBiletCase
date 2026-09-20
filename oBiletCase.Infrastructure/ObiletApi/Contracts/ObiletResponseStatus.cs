namespace oBiletCase.Infrastructure.oBiletAPI.Contracts;

/// <summary>
/// Listelenen ResponseStatus değerleri.
/// </summary>
public enum ObiletResponseStatus
{
    Success,
    InvalidDepartureDate,
    InvalidRoute,
    InvalidLocation,
    Timeout,
}
