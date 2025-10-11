using Issueneter.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Issueneter.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddScoped<ClientFactory>()
            .AddScoped<ProviderFactory>();

        return serviceCollection;
    }
}