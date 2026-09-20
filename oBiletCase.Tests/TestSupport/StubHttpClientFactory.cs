namespace oBiletCase.Tests.TestSupport;

/// <summary>
/// Testlerde tek bir sabit <see cref="HttpClient"/> döndüren sahte
/// <see cref="IHttpClientFactory"/> implementasyonu.
/// </summary>
public sealed class StubHttpClientFactory(HttpClient client) : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => client;
}
