namespace oBiletCase.Web.AppUser;

/// <summary>
/// Mevcut isteğin uygulama-içi kullanıcı kimliğine erişim sağlar. Bu kimlik
/// yalnızca sunucu tarafı ilişkilendirme (ör. Obilet session önbelleği)
/// içindir; kullanıcıya gösterilmez, Obilet'e gönderilmez.
/// </summary>
public interface IAppUserContext
{
    /// <summary>
    /// <see cref="AppUserIdentityMiddleware"/> tarafından ayarlanan opak
    /// kullanıcı kimliği.
    /// </summary>
    string AppUserId { get; }
}
