using FluentMigrator.Runner;
using Issueneter.Domain.Interfaces.Connection;
using Issueneter.Domain.Interfaces.Repos;
using Issueneter.Infrastructure.Database.Connection;
using Issueneter.Infrastructure.Database.Repos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Issueneter.Infrastructure.Database;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetValue<string>("Database:ConnectionString");
        services.Configure<DbConnectionFactoryConfiguration>(configuration.GetSection("Database"));

        services.AddFluentMigratorCore()
            .ConfigureRunner(r => r
                .AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(ServiceCollectionExtensions).Assembly).For.Migrations())
            .AddLogging(l => l.AddFluentMigratorConsole());

        return services
            .AddScoped<IProviderSnapshotRepo, ProviderSnapshotRepo>()
            .AddScoped<IWorkerConfigurationRepo, WorkerConfigurationRepo>()
            .AddScoped<DbConnectionFactory>()
            .AddScoped<ITransactionProvider>(sp => sp.GetRequiredService<DbConnectionFactory>());
    }

    public static void RunMigrations(this IServiceProvider services)
    {
        var runner = services.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }
}