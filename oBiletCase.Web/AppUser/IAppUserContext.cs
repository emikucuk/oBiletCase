namespace oBiletCase.Web.AppUser;

/// <summary>
/// Mevcut isteğin uygulama-içi kullanıcı kimliğine erişim sağlar. Sadece uygulama içinde kullanılır.
/// </summary>
public interface IAppUserContext
{
    string AppUserId { get; }
}
