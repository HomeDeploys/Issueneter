using Issueneter.Domain.Interfaces.Services;
using Issueneter.Infrastructure.Telegram.Configuration;
using Issueneter.Infrastructure.Telegram.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Issueneter.Infrastructure.Telegram;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelegram(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.Configure<TelegramClientConfiguration>(configuration.GetSection(nameof(TelegramClientConfiguration)));

        return serviceCollection.AddScoped<IClient, TelegramClient>();
    }
}