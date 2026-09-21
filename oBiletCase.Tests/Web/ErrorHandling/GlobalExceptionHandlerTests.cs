using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using oBiletCase.Web.ErrorHandling;

namespace oBiletCase.Tests.Web.ErrorHandling;

public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler = new(NullLogger<GlobalExceptionHandler>.Instance);

    [Fact]
    public async Task TryHandleAsync_json_bekleyen_istekte_yaniti_kendisi_yazar_ve_true_doner()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Accept = "application/json";
        context.Response.Body = new MemoryStream();

        var handled = await _handler.TryHandleAsync(context, new InvalidOperationException("boom"), CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.StartsWith("application/json", context.Response.ContentType);
    }

    [Fact]
    public async Task TryHandleAsync_html_istekte_islemedigini_belirtip_pipeline_e_birakir()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Accept = "text/html";

        var handled = await _handler.TryHandleAsync(context, new InvalidOperationException("boom"), CancellationToken.None);

        Assert.False(handled);
    }
}
