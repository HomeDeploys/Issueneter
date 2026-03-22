namespace Issueneter.Infrastructure.Telegram.Configuration;

internal class TelegramClientConfiguration
{
    public required string Token { get; set; }
    public required List<long> AllowedUsers { get; set; }
    public ProxyConfiguration? ProxyConfiguration { get; set; }
}

internal class ProxyConfiguration
{
    public required string Address { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}