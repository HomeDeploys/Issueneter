namespace Issueneter.Infrastructure.Telegram.Configuration;

internal class TelegramClientConfiguration
{
    public required string Token { get; set; }
    public required List<long> AllowedUsers { get; set; }
}