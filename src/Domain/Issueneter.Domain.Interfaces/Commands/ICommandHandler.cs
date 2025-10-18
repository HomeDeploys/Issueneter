using Issueneter.Domain.Models;

namespace Issueneter.Domain.Interfaces.Commands;

public interface ICommandHandler
{
    bool CanHandle(Command command);
    Task<string> Handle(Command command, CancellationToken token);
}