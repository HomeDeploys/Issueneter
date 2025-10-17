using Issueneter.Domain.Models;

namespace Issueneter.Domain.Interfaces.Commands;

public interface ICommandHandlerFactory
{
    bool TryGet(Command command, out ICommandHandler handler);
}