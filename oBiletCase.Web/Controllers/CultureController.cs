using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using oBiletCase.Web.Localization;

namespace oBiletCase.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public sealed class CultureController : Controller
{
    private static readonly TimeSpan CookieLifetime = TimeSpan.FromDays(365);

    [HttpGet]
    public IActionResult Set(string culture, string returnUrl)
    {
        if (culture is not (AppCultures.Turkish or AppCultures.English))
        {
            culture = AppCultures.Turkish;
        }

        Response.Cookies.Append(
            AppCultures.CookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.Add(CookieLifetime), SameSite = SameSiteMode.Lax });

        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
    }
}
