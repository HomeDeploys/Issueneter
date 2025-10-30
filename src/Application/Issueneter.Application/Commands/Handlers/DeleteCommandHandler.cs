using Issueneter.Domain.Interfaces.Commands;
using Issueneter.Domain.Interfaces.Repos;
using Issueneter.Domain.Interfaces.Services;
using Issueneter.Domain.Models;

namespace Issueneter.Application.Commands.Handlers;

internal class DeleteCommandHandler : ICommandHandler
{
    private readonly IWorkerConfigurationRepo _workerConfigurationRepo;
    private readonly IScheduler _scheduler;

    public DeleteCommandHandler(IWorkerConfigurationRepo workerConfigurationRepo, IScheduler scheduler)
    {
        _workerConfigurationRepo = workerConfigurationRepo;
        _scheduler = scheduler;
    }

    public bool CanHandle(Command command)
    {
        return command.Name.Equals("delete", StringComparison.InvariantCultureIgnoreCase);
    }

    public async Task<string> Handle(Command command, CancellationToken token)
    {
        if (command.WorkerId.IsEmpty())
        {
            return "WorkerId is required for delete command";
        }
        
        if (command.Parameters.Any())
        {
            return "Delete command can't have parameters";
        }

        await _workerConfigurationRepo.Delete(command.WorkerId, token);
        await _scheduler.Deschedule(command.WorkerId);
        
        return $"WorkerId {command.WorkerId} deleted";
    }
}