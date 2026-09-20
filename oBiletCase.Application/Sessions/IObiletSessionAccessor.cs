namespace oBiletCase.Application.Sessions;

/// <summary>
/// Uygulamanın her kullanıcısı için bir Obilet session'ının edinilmesinden
/// ve yeniden kullanılmasından sorumlu sözleşme. Session'ın Obilet API'sinden
/// nasıl alındığı, önbelleklendiği veya yenilendiği Infrastructure'ın
/// sorumluluğundadır; Application yalnızca bu sözleşmeyi bilir.
/// </summary>
public interface IObiletSessionAccessor
{
    /// <param name="appUserId">
    /// Uygulamanın kendi kullanıcı tanımlama mekanizmasından (cookie vb.)
    /// gelen, kullanıcıya özgü opak kimlik. Obilet'in session-id/device-id
    /// çiftiyle karıştırılmamalıdır; yalnızca session'ı kullanıcı bazında
    /// izole etmek için kullanılır.
    /// </param>
    Task<ObiletSession> GetOrCreateSessionAsync(string appUserId, CancellationToken cancellationToken);
}
