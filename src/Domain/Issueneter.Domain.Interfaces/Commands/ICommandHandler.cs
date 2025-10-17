using Issueneter.Domain.Models;

namespace Issueneter.Domain.Interfaces.Commands;

public interface ICommandHandler
{
    ValidationResult Validate(Command command);
    Task<string> Handle(Command command, CancellationToken token);
}