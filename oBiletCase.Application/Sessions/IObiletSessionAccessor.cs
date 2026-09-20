namespace oBiletCase.Application.Sessions;

public interface IObiletSessionAccessor
{
    Task<ObiletSession> GetOrCreateSessionAsync(string appUserId, CancellationToken cancellationToken);
}
