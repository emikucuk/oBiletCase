namespace oBiletCase.Web.AppUser;

/// <summary>
/// Uygulamanın kendi kullanıcı tanımlama cookie'siyle ilgili sabitler.
/// Bu kimlik yalnızca sunucu tarafında (ör. Obilet session'ını kullanıcı
/// bazında ilişkilendirmek için) kullanılır; Obilet'in kendi
/// session-id/device-id çiftiyle karıştırılmamalıdır (bkz. AGENTS.md
/// "API ve güvenlik" bölümü).
/// </summary>
public static class AppUserIdentity
{
    /// <summary>Tarayıcıya yazılan, uygulamaya özgü kullanıcı kimlik cookie'sinin adı.</summary>
    public const string CookieName = "oBiletCase.UserId";

    /// <summary>İstek işlenirken kullanıcı kimliğinin taşındığı <see cref="HttpContext.Items"/> anahtarı.</summary>
    public const string HttpContextItemsKey = "AppUserId";
}
