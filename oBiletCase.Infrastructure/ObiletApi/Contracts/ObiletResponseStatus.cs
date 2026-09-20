namespace oBiletCase.Infrastructure.oBiletAPI.Contracts;

/// <summary>
/// docs/obilet.com-API-Integration-Specs.pdf Appendix A'da listelenen
/// ResponseStatus değerleri.
/// </summary>
public enum ObiletResponseStatus
{
    /// <summary>Dokümante edilmemiş/beklenmeyen bir status değeri geldi.</summary>
    Unknown = 0,
    Success,
    InvalidDepartureDate,
    InvalidRoute,
    InvalidLocation,
    Timeout,
}
