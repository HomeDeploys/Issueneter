using Issueneter.Application.Commands;
using Issueneter.Application.Services;
using Issueneter.Domain.Interfaces.Commands;
using Issueneter.Domain.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Issueneter.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddScoped<ICommandHandlerFactory, CommandHandlerFactory>()
            .AddScoped<ICommandHandler, CreateCommandHandler>()
            .AddScoped<IWorker, Worker>()
            .AddScoped<IClientFactory, ClientFactory>()
            .AddScoped<IProviderFactory, ProviderFactory>();

        return serviceCollection;
    }
}