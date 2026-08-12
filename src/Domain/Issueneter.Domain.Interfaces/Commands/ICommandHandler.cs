using Issueneter.Domain.Models;

namespace Issueneter.Domain.Interfaces.Commands;

public interface ICommandHandler
{
    string Name { get; }
    string Description { get; }
    IReadOnlyCollection<CommandArgumentInfo> Arguments { get; }
    string Example { get; }

    bool CanHandle(Command command);
    Task<string> Handle(Command command, CancellationToken token);
}