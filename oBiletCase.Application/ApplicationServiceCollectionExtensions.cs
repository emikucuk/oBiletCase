using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using oBiletCase.Application.Journeys;

namespace oBiletCase.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IValidator<JourneySearchCriteria>, JourneySearchCriteriaValidator>();

        return services;
    }
}
