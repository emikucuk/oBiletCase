using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using oBiletCase.Application.Journeys;
using oBiletCase.Application.Locations;
using oBiletCase.Application.Sessions;
using oBiletCase.Infrastructure.oBiletAPI;

namespace oBiletCase.Infrastructure;

/// <summary>
/// Kendi servis kayıtlarının tanımlandığı extension. API bağlantısı için gereken servis ve options tanımlarını yapar.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<oBiletAPIOptions>()
            .Bind(configuration.GetSection(oBiletAPIOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient(oBiletAPIOptions.HttpClientName, (provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<oBiletAPIOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", options.ApiClientToken);
        });

        services.AddMemoryCache();
        services.AddSingleton<ObiletApiClient>();
        services.AddSingleton<IObiletSessionAccessor, ObiletSessionAccessor>();
        services.AddSingleton<IBusLocationService, BusLocationService>();
        services.AddSingleton<IBusJourneyService, BusJourneyService>();

        return services;
    }
}
