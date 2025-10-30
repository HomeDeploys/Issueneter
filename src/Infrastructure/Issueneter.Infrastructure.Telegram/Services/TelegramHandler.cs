using Issueneter.Domain.Interfaces.Commands;
using Issueneter.Infrastructure.Telegram.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Issueneter.Infrastructure.Telegram.Services;

internal class TelegramHandler
{
    private readonly TelegramBotClient _client;
    private readonly CancellationTokenSource _tokenSource;
    private readonly ICommandParser _commandParser;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TelegramHandler> _logger;
    private readonly HashSet<long> _usersWhitelist;

    public TelegramHandler(
        IOptions<TelegramClientConfiguration> configuration,
        ICommandParser commandParser, 
        IServiceScopeFactory scopeFactory, 
        ILogger<TelegramHandler> logger)
    {
        _tokenSource = new CancellationTokenSource();
        _client = new TelegramBotClient(configuration.Value.Token, cancellationToken: _tokenSource.Token);
        _commandParser = commandParser;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _usersWhitelist = configuration.Value.AllowedUsers.ToHashSet();
    }

    public void Start()
    {
        _client.OnMessage += OnMessage;
    }
    
    public void Stop()
    {
        _client.OnMessage -= OnMessage;
        _tokenSource.Cancel();
    }

    public async Task OnMessage(Message message, UpdateType type)
    {
        if (type != UpdateType.Message) return;
        if (string.IsNullOrEmpty(message.Text)) return;
        if (!_usersWhitelist.Contains(message.From?.Id ?? 0)) return;

        try
        {
            var reply = await HandleMessage(message.Text, _tokenSource.Token);
            await _client.SendMessage(message.Chat.Id, reply, messageThreadId: message.MessageThreadId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while handling message {messageText}", message.Text);
            await _client.SendMessage(message.Chat.Id, "Error processing command, try again later", messageThreadId: message.MessageThreadId);
        }
    }

    // TODO: Error handling
    private async Task<string> HandleMessage(string message, CancellationToken token)
    {
        var parseResult = _commandParser.Parse(message);

        if (!parseResult.IsSuccess)
        {
            return $"Error handling message: {parseResult.Error}";
        }
        
        var command = parseResult.Entity!;
        using var scope = _scopeFactory.CreateScope();
        
        var factory = scope.ServiceProvider.GetRequiredService<ICommandHandlerFactory>();
        var handler = factory.Get(command);
        
        if (handler is null)
        {
            return $"No handler exists for command {command.Name}";
        }

        return await handler.Handle(command, token);
    }
}