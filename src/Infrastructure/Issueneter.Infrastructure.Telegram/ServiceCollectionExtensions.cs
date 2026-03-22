using System.Net;
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
            .AddSingleton(sp =>
            {
                var configuration = sp.GetRequiredService<IOptions<TelegramClientConfiguration>>().Value;

                if (configuration.ProxyConfiguration is null)
                {
                    return new TelegramBotClient(configuration.Token);
                }

                var webProxy = new WebProxy(configuration.ProxyConfiguration.Address)
                {
                    Credentials = new NetworkCredential(configuration.ProxyConfiguration.Username,
                        configuration.ProxyConfiguration.Password)
                };

                var httpClientHandler = new HttpClientHandler
                {
                    Proxy = webProxy,
                };
                var httpClient = new HttpClient(httpClientHandler);
                return new TelegramBotClient(configuration.Token, httpClient);
            })
            .AddSingleton<ITelegramBotClient>(sp => sp.GetRequiredService<TelegramBotClient>())
            .AddSingleton<TelegramHandler>()
            .AddScoped<IClient, TelegramClient>();
    }

    public static void RunTelegramHandler(this IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<TelegramHandler>().Start();
    }
}