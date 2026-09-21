using Microsoft.AspNetCore.Diagnostics;

namespace oBiletCase.Web.ErrorHandling;


public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "Beklenmeyen bir hata oluştu. RequestId={RequestId}, Path={Path}",
            httpContext.TraceIdentifier,
            httpContext.Request.Path.Value);

        if (!WantsJson(httpContext))
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(
            new { requestId = httpContext.TraceIdentifier },
            cancellationToken);

        return true;
    }

    private static bool WantsJson(HttpContext context) =>
        context.Request.Headers.Accept
            .Any(value => value is not null && value.Contains("application/json", StringComparison.OrdinalIgnoreCase));
}
