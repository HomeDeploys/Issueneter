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
    private static readonly HashSet<string> Fields = typeof(CreateCommand).GetProperties().Select(x => x.Name).ToHashSet();
    
    public static ParseResult<CreateCommand> Parse(Command command)
    {
        foreach (var field in Fields)
        {
            if (!command.Parameters.ContainsKey(field))
            {
                return ParseResult<CreateCommand>.Fail($"Missing required parameter {field}");
            }
        }

        foreach (var parameter in command.Parameters.Keys)
        {
            if (!Fields.Contains(parameter))
            {
                return ParseResult<CreateCommand>.Fail($"Invalid parameter {parameter}");
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
}