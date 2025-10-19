using Issueneter.Domain.Interfaces.Commands;
using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;

namespace Issueneter.Application.Commands;

internal class CommandParser : ICommandParser
{
    public ParseResult<Command> Parse(string command)
    {
        //TODO: Rewrite with spans
        if (string.IsNullOrWhiteSpace(command))
        {
            return ParseResult<Command>.Fail("Command is empty");
        }
        
        var lines = command.Split(Environment.NewLine);
        var commandName = lines[0].Trim();
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
        
        return ParseResult<Command>.Success(new Command(commandName, arguments));
    }
}