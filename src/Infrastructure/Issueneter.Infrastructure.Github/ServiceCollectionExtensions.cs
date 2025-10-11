using Issueneter.Domain.Interfaces.Services;
using Issueneter.Infrastructure.Github.Configuration;
using Issueneter.Infrastructure.Github.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Issueneter.Infrastructure.Github;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGithub(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GithubClientConfiguration>(configuration.GetSection(nameof(GithubClientConfiguration)));
        
        return services
            .AddScoped<GithubClient>()
            .AddScoped<IEntityProvider, GithubProvider>();
    }
}