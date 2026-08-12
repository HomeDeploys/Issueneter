using Issueneter.Common.Extensions;
using Issueneter.Domain.Enums;
using Issueneter.Domain.Interfaces.Commands;
using Issueneter.Domain.Interfaces.Services;
using Issueneter.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Issueneter.Application.Commands.Handlers;

internal class HelpCommandHandler : ICommandHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<IEntityProvider> _providers;
    private readonly IEnumerable<IClient> _clients;

    public HelpCommandHandler(IServiceProvider serviceProvider, IEnumerable<IEntityProvider> providers, IEnumerable<IClient> clients)
    {
        _serviceProvider = serviceProvider;
        _providers = providers;
        _clients = clients;
    }

    public string Name => "help";

    public string Description => "Shows available commands, providers and clients, or details about a specific one.";

    public IReadOnlyCollection<CommandArgumentInfo> Arguments { get; } =
    [
        new("Type", "What to show help for: command, provider or client. Omit to list all commands.", IsRequired: false),
        new("Name", "Name of the specific command/provider/client to show details for. Omit to list all.", IsRequired: false)
    ];

    public string Example =>
        """
        help
        help command create
        help provider Github
        help client Telegram
        """;

    public bool CanHandle(Command command)
    {
        return command.Name.Equals("help", StringComparison.InvariantCultureIgnoreCase);
    }

    public Task<string> Handle(Command command, CancellationToken token)
    {
        var type = command.Parameters.GetValueOrDefault("Type");
        var name = command.Parameters.GetValueOrDefault("Name");

        if (string.IsNullOrEmpty(type) || type.Equals("command", StringComparison.InvariantCultureIgnoreCase))
        {
            // IEnumerable<ICommandHandler> is resolved lazily here, not constructor-injected: this
            // handler is itself registered as an ICommandHandler, so constructor-injecting the same
            // collection would create a circular DI dependency (building the enumerable requires
            // building this handler, which requires the enumerable).
            var commandHandlers = _serviceProvider.GetServices<ICommandHandler>().ToList();
            return Task.FromResult(HandleCommandHelp(commandHandlers, name));
        }

        if (type.Equals("provider", StringComparison.InvariantCultureIgnoreCase))
        {
            return Task.FromResult(HandleProviderHelp(name));
        }

        if (type.Equals("client", StringComparison.InvariantCultureIgnoreCase))
        {
            return Task.FromResult(HandleClientHelp(name));
        }

        return Task.FromResult($"Unknown help type: {type}. Expected 'command', 'provider' or 'client'.");
    }

    private static string HandleCommandHelp(IReadOnlyCollection<ICommandHandler> handlers, string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return HelpFormatter.FormatCommandList(handlers);
        }

        var handler = handlers.FirstOrDefault(h => h.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        return handler is null
            ? $"Command not found: {name}"
            : HelpFormatter.FormatCommandDetail(handler);
    }

    private string HandleProviderHelp(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return HelpFormatter.FormatProviderList(_providers);
        }

        if (!Enum.TryParseSafe<ProviderType>(name, out var providerType))
        {
            return $"Unknown provider: {name}";
        }

        var provider = _providers.FirstOrDefault(p => p.Type == providerType);
        return provider is null
            ? $"Provider not found: {name}"
            : HelpFormatter.FormatProviderDetail(provider);
    }

    private string HandleClientHelp(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return HelpFormatter.FormatClientList(_clients);
        }

        if (!Enum.TryParseSafe<ClientType>(name, out var clientType))
        {
            return $"Unknown client: {name}";
        }

        var client = _clients.FirstOrDefault(c => c.Type == clientType);
        return client is null
            ? $"Client not found: {name}"
            : HelpFormatter.FormatClientDetail(client);
    }
}
