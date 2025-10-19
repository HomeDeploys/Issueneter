using Hangfire;
using Hangfire.PostgreSql;
using Issueneter.Domain.Interfaces.Services;
using Issueneter.Infrastructure.Background.Services;
using Issueneter.Infrastructure.Background.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Issueneter.Infrastructure.Background;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackground(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetValue<string>("Database:ConnectionString");
        
        services.AddHangfireServer();
        services.AddHangfire((provider, config) =>
        {
            // TODO: Use existing connection for transactions
            config.UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connectionString));
            config.UseActivator(new ContainerJobActivator(provider.GetRequiredService<IServiceScopeFactory>()));
        });

        services.AddScoped<IScheduler, Scheduler>();
        
        return services;
    }
}