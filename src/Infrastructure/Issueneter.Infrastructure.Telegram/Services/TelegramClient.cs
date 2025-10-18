using System.Text.RegularExpressions;
using Issueneter.Common.Exceptions;
using Issueneter.Domain.Enums;
using Issueneter.Domain.Interfaces.Services;
using Issueneter.Domain.ValueObjects;
using Issueneter.Infrastructure.Telegram.Configuration;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace Issueneter.Infrastructure.Telegram.Services;

internal partial class TelegramClient : IClient
{
    [GeneratedRegex(@"^(?<chatId>[0-9]+)\/(?<threadId>[0-9]+)?$", RegexOptions.Compiled)]
    private static partial Regex TargetChatRegex();

    private readonly TelegramBotClient _client;

    public TelegramClient(IOptions<TelegramClientConfiguration> configuration)
    {
        _client = new TelegramBotClient(configuration.Value.Token);
    }

    public ClientType Type =>  ClientType.Telegram;
    public ValidationResult Validate(string target)
    {
        var match = TargetChatRegex().IsMatch(target);

        if (!match)
        {
            return ValidationResult.Fail("Telegram target must be in format <chatId> or <chatId>/<threadId>");
        }
        
        return ValidationResult.Success;
    }

    public async Task Send(string target, string message, CancellationToken token)
    {
        var regex = TargetChatRegex();
        var match = regex.Match(target);

        if (!match.Success)
        {
            throw new InvalidTargetException($"Invalid telegram target: {target}");
        }
        
        var chatId = long.Parse(match.Groups["chatId"].Value);
        
        int? threadId = null;
        if (match.Groups["threadId"].Success)
        {
            threadId = int.Parse(match.Groups["threadId"].Value);
        }
        
        await _client.SendMessage(chatId, message, messageThreadId: threadId, cancellationToken: token);
    }
}