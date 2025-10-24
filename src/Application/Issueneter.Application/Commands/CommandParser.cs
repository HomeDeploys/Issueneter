using System.Text.RegularExpressions;
using Issueneter.Domain.Interfaces.Commands;
using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;

namespace Issueneter.Application.Commands;

internal partial class CommandParser : ICommandParser
{
    [GeneratedRegex(@"(?<name>[a-zA-Z]+)( (?<workerId>[0-9]+))?")]
    private static partial Regex CommandNameRegex();
    
    public ParseResult<Command> Parse(string command)
    {
        //TODO: Rewrite with spans
        if (string.IsNullOrWhiteSpace(command))
        {
            return ParseResult<Command>.Fail("Command is empty");
        }
        
        var lines = command.Split('\n');
        var commandNameMatch = CommandNameRegex().Match(lines[0]);

        if (!commandNameMatch.Success)
        {
            return ParseResult<Command>.Fail($"Invalid command name: {lines[0]}");
        }

        var commandName = commandNameMatch.Groups["name"].Value;
        var workerId = WorkerId.Empty;
        if (commandNameMatch.Groups["workerId"].Success)
        {
            workerId = new WorkerId(long.Parse(commandNameMatch.Groups["workerId"].Value));
        }
        var arguments = new Dictionary<string, string>();
        
        foreach (var line in lines.Skip(1))
        {
            var arg = line.Split(':', 2);
            if (arg.Length != 2)
            {
                return ParseResult<Command>.Fail($"Invalid argument format: {line} \n Expected format: \"Name: Value\"");
            }
            
            arguments[arg[0].Trim()] =  arg[1].Trim();
        }
        
        return ParseResult<Command>.Success(new Command(commandName, workerId, arguments));
    }
}