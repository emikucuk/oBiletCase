using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using oBiletCase.Application.Journeys;
using oBiletCase.Application.Locations;
using oBiletCase.Application.Sessions;
using oBiletCase.Infrastructure.Localization;
using oBiletCase.Infrastructure.oBiletAPI;
using oBiletCase.Infrastructure.Persistence;

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

        var connectionString = configuration.GetConnectionString(AppDbContext.ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"'ConnectionStrings:{AppDbContext.ConnectionStringName}' yapılandırılmamış (user-secrets veya ortam değişkeni).");
        }

        // Tüketiciler singleton olduğu için scoped DbContext yerine her işlemde kısa ömürlü context üreten factory.
        services.AddDbContextFactory<AppDbContext>(options => options.UseNpgsql(connectionString));

        services.AddMemoryCache();
        services.AddSingleton<IObiletApiClient, ObiletApiClient>();
        services.AddSingleton<IObiletSessionAccessor, ObiletSessionAccessor>();
        services.AddSingleton<IBusLocationService, BusLocationService>();
        services.AddSingleton<IBusJourneyService, BusJourneyService>();
        services.AddSingleton<IFeatureTranslationStore, FeatureTranslationStore>();

        // AddLocalization'ın resx tabanlı factory'si TryAdd ile eklendiği için çağrı sırasından bağımsız olarak değiştirilir.
        services.AddSingleton<LocalizationResourceCatalog>();
        services.Replace(ServiceDescriptor.Singleton<IStringLocalizerFactory, DbStringLocalizerFactory>());

        return services;
    }

    /// <summary>
    /// Bekleyen EF Core migration'larını uygular (tablolar + seed çevirileri).
    /// </summary>
    public static async Task MigrateDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var dbContextFactory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
