using Issueneter.Domain.Models;

namespace Issueneter.Domain.Interfaces.Commands;

public interface ICommandParser
{
    CommandParseResult Parse(string command);
}

public record CommandParseResult(Command? Command, string Error)
{
    public bool IsSuccess => Command is not null;
    
    public static CommandParseResult Success(Command command) => new(command, string.Empty);
    public static CommandParseResult Fail(string error) => new(null, error);
}