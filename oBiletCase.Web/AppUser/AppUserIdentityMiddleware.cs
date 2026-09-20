namespace oBiletCase.Web.AppUser;

/// <summary>
/// Her istekte, uygulamanın kendi kullanıcı tanımlama cookie'sinin var
/// olduğundan emin olur; yoksa oluşturur. Bu kimlik, Obilet session'ını
/// kullanıcı bazında ilişkilendirmek için kullanılır (bkz.
/// <see cref="AppUserIdentity"/>).
/// </summary>
public sealed class AppUserIdentityMiddleware(RequestDelegate next)
{
    private static readonly TimeSpan CookieLifetime = TimeSpan.FromDays(365);

    public async Task InvokeAsync(HttpContext context)
    {
        var appUserId = context.Request.Cookies[AppUserIdentity.CookieName];

        if (string.IsNullOrWhiteSpace(appUserId))
        {
            appUserId = Guid.NewGuid().ToString("N");

            context.Response.Cookies.Append(AppUserIdentity.CookieName, appUserId, new CookieOptions
            {
                HttpOnly = true,
                Secure = !context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment(),
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.Add(CookieLifetime),
            });
        }

        context.Items[AppUserIdentity.HttpContextItemsKey] = appUserId;

        await next(context);
    }
}
