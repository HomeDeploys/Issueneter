using Issueneter.Domain.Interfaces.Commands;
using Issueneter.Domain.Interfaces.Repos;
using Issueneter.Domain.Models;

namespace Issueneter.Application.Commands.Handlers;

internal class GetCommandHandler : ICommandHandler
{
    private readonly IWorkerConfigurationRepo _workerConfigurationRepo;

    public GetCommandHandler(IWorkerConfigurationRepo workerConfigurationRepo)
    {
        _workerConfigurationRepo = workerConfigurationRepo;
    }

    public bool CanHandle(Command command)
    {
        return command.Name.Equals("get", StringComparison.InvariantCultureIgnoreCase);
    }

    public async Task<string> Handle(Command command, CancellationToken token)
    {
        if (command.WorkerId.IsEmpty())
        {
            return "WorkerId is required for get command";
        }
        
        if (command.Parameters.Any())
        {
            return "Get command can't have parameters";
        }

        var configuration = await _workerConfigurationRepo.Get(command.WorkerId, token);

        if (configuration is null)
        {
            return $"Worker with id {command.WorkerId} not found";
        }

        return configuration.ToString();
    }
}