using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;

namespace Issueneter.Application.Commands.Models;

internal record UpdateCommand(
    string? ProviderTarget,
    string? Schedule,
    string? Filter,
    string? ClientTarget,
    string? Template)
{
    private static readonly HashSet<string> Fields = typeof(UpdateCommand).GetProperties().Select(x => x.Name).ToHashSet();
    
    public static ParseResult<UpdateCommand> Parse(Command command)
    {
        bool hasAtLeastOneParameter = false;

        foreach (var field in Fields)
        {
            if (command.Parameters.ContainsKey(field))
            {
                hasAtLeastOneParameter = true;
            }
        }

        if (!hasAtLeastOneParameter)
        {
            return ParseResult<UpdateCommand>.Fail($"Update command must have at least one parameter");
        }
        
        foreach (var parameter in command.Parameters.Keys)
        {
            if (!Fields.Contains(parameter))
            {
                return ParseResult<UpdateCommand>.Fail($"Invalid parameter {parameter}");
            }
        }

        return ParseResult<UpdateCommand>.Success(new UpdateCommand(
            ProviderTarget: command.Parameters.GetValueOrDefault(nameof(ProviderTarget)),
            Schedule: command.Parameters[nameof(Schedule)],
            Filter: command.Parameters[nameof(Filter)],
            ClientTarget: command.Parameters[nameof(ClientTarget)],
            Template: command.Parameters[nameof(Template)]
        ));
    }
}