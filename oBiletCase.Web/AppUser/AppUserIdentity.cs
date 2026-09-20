namespace oBiletCase.Web.AppUser;

/// <summary>
/// Uygulamanın kendi kullanıcı tanımlama cookie'siyle ilgili sabitler.
/// </summary>
public static class AppUserIdentity
{
    /// Tarayıcıya yazılan, cookie adı.
    public const string CookieName = "oBiletCase.UserId";

    /// Kullanıcı kimliğinin taşındığı HttpContext.Items anahtarı.
    public const string HttpContextItemsKey = "AppUserId";
}
