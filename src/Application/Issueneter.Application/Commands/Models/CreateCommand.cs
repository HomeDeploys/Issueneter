using Issueneter.Domain.Interfaces.Services;
using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;

namespace Issueneter.Application.Commands.Models;

internal record CreateCommand(
    string ProviderType,
    string ProviderTarget,
    string Schedule,
    string Filter,
    string ClientType,
    string ClientTarget,
    string Template)
{
    private static readonly string[] Fields = typeof(CreateCommand).GetFields().Select(x => x.Name).ToArray();
    
    public static ParseResult<CreateCommand> Parse(Command command)
    {
        foreach (var field in Fields)
        {
            if (!command.Parameters.ContainsKey(field))
            {
                return ParseResult<CreateCommand>.Fail($"Missing required parameter {field}");
            }
        }

        return ParseResult<CreateCommand>.Success(new CreateCommand(
            ProviderType: command.Parameters[nameof(ProviderType)],
            ProviderTarget: command.Parameters[nameof(ProviderTarget)],
            Schedule: command.Parameters[nameof(Schedule)],
            Filter: command.Parameters[nameof(Filter)],
            ClientType: command.Parameters[nameof(ClientType)],
            ClientTarget: command.Parameters[nameof(ClientTarget)],
            Template: command.Parameters[nameof(Template)]
        ));
    }

    public ParseResult<ProviderInfo> ParseProvider(IProviderFactory factory, out IEntityProvider? provider)
    {
        return CommandHelpers.ParseProvider(ProviderType, ProviderTarget, factory, out provider);
    }
    
    public ParseResult<ClientInfo> ParseClient(IClientFactory factory)
    {
        return CommandHelpers.ParseClient(ClientType, ClientTarget, factory);
    }
}