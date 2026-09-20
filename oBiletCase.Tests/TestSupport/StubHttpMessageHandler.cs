namespace oBiletCase.Tests.TestSupport;

/// <summary>
/// Gerçek ağ çağrısı yapmadan HttpClient testleri yazabilmek için kullanılan
/// sahte (fake) HttpMessageHandler. Her istek, verilen delegate'e devredilir.
/// </summary>
public sealed class StubHttpMessageHandler(
    Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> responder)
    : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(responder(request, cancellationToken));
}
