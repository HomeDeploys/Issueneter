using Issueneter.Domain.Interfaces.Commands;
using Issueneter.Domain.Models;

namespace Issueneter.Application.Commands;

internal class CommandHandlerFactory : ICommandHandlerFactory
{
    private readonly IEnumerable<ICommandHandler> _commandHandlers;

    public CommandHandlerFactory(IEnumerable<ICommandHandler> commandHandlers)
    {
        _commandHandlers = commandHandlers;
    }

    public ICommandHandler? Get(Command command)
    {
        return _commandHandlers.FirstOrDefault(h => h.CanHandle(command));
    }
}