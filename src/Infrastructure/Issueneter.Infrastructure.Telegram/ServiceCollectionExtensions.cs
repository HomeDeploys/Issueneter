using Issueneter.Domain.Interfaces.Services;
using Issueneter.Infrastructure.Telegram.Configuration;
using Issueneter.Infrastructure.Telegram.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace Issueneter.Infrastructure.Telegram;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelegram(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.Configure<TelegramClientConfiguration>(configuration.GetSection(nameof(TelegramClientConfiguration)));

        return serviceCollection
            .AddSingleton(sp => new TelegramBotClient(sp.GetRequiredService<IOptions<TelegramClientConfiguration>>().Value.Token))
            .AddSingleton<ITelegramBotClient>(sp => sp.GetRequiredService<TelegramBotClient>())
            .AddSingleton<TelegramHandler>()
            .AddScoped<IClient, TelegramClient>();
    }

    public static void RunTelegramHandler(this IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<TelegramHandler>().Start();
    }
}