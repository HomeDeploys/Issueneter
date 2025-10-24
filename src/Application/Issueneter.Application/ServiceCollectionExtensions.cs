using Issueneter.Application.Commands;
using Issueneter.Application.Commands.Handlers;
using Issueneter.Application.Services;
using Issueneter.Domain.Interfaces.Commands;
using Issueneter.Domain.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Issueneter.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<ICommandParser, CommandParser>()
            .AddSingleton<IMessageFormatter, MessageFormatter>()
            .AddScoped<WorkerConfigurationValidator>()
            .AddScoped<ICommandHandlerFactory, CommandHandlerFactory>()
            .AddScoped<IWorker, Worker>()
            .AddScoped<IClientFactory, ClientFactory>()
            .AddScoped<IProviderFactory, ProviderFactory>()
            .AddCommandHandlers();
    }

    private static IServiceCollection AddCommandHandlers(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddScoped<ICommandHandler, CreateCommandHandler>()
            .AddScoped<ICommandHandler, UpdateCommandHandler>()
            .AddScoped<ICommandHandler, GetCommandHandler>();
    }
}