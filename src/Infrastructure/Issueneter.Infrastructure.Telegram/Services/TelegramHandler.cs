using System.Text;
using Issueneter.Domain.Interfaces.Commands;
using Issueneter.Infrastructure.Telegram.Configuration;
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
    private readonly ICommandHandlerFactory _commandHandlerFactory;

    public TelegramHandler(
        IOptions<TelegramClientConfiguration> configuration,
        ICommandParser commandParser, 
        ICommandHandlerFactory commandHandlerFactory)
    {
        _tokenSource = new CancellationTokenSource();
        _client = new TelegramBotClient(configuration.Value.Token, cancellationToken: _tokenSource.Token);
        _commandParser = commandParser;
        _commandHandlerFactory = commandHandlerFactory;
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
        
        var reply = await HandleMessage(message.Text, _tokenSource.Token);
        await _client.SendMessage(message.Chat.Id, reply);
    }

    // TODO: Error handling
    private async Task<string> HandleMessage(string message, CancellationToken token)
    {
        var parseResult = _commandParser.Parse(message);

        if (!parseResult.IsSuccess)
        {
            return $"Error handling message: {parseResult.Error}";
        }
        
        var command = parseResult.Command!;
        var handlerExists = _commandHandlerFactory.TryGet(command,  out var handler);

        if (!handlerExists)
        {
            return $"No handler exists for command {command.Name}";
        }

        var validationResult = handler.Validate(command);

        if (!validationResult.IsSuccess)
        {
            var errorBuilder = new StringBuilder();
            errorBuilder.AppendLine("Invalid command:");
            foreach (var error in validationResult.Errors)
            {
                errorBuilder.AppendLine(error);
            }
            return errorBuilder.ToString();
        }

        return await handler.Handle(command, token);
    }
}