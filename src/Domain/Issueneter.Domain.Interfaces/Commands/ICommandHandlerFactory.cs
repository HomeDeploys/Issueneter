using Issueneter.Domain.Models;

namespace Issueneter.Domain.Interfaces.Commands;

public interface ICommandHandlerFactory
{
    ICommandHandler? Get(Command command);
}