using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;

namespace Issueneter.Domain.Interfaces.Commands;

public interface ICommandParser
{
    ParseResult<Command> Parse(string command);
}