namespace oBiletCase.Tests.TestSupport;

/// Testlerde tek bir sabit HttpClient döndüren sahte IHttpClientFactory implementasyonu.
public sealed class StubHttpClientFactory(HttpClient client) : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => client;
}
