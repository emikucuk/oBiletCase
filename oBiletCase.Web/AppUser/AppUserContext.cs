namespace oBiletCase.Web.AppUser;

/// <inheritdoc cref="IAppUserContext"/>
public sealed class AppUserContext(IHttpContextAccessor httpContextAccessor) : IAppUserContext
{
    public string AppUserId
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("Aktif bir HttpContext yok.");

            if (httpContext.Items[AppUserIdentity.HttpContextItemsKey] is not string appUserId)
            {
                throw new InvalidOperationException(
                    $"{nameof(AppUserIdentityMiddleware)} pipeline'a eklenmemiş görünüyor.");
            }

            return appUserId;
        }
    }
}
