using Issueneter.Application.Commands.Models;
using Issueneter.Domain.Interfaces.Commands;
using Issueneter.Domain.Interfaces.Connection;
using Issueneter.Domain.Interfaces.Repos;
using Issueneter.Domain.Interfaces.Services;
using Issueneter.Domain.Models;

namespace Issueneter.Application.Commands.Handlers;

internal class UpdateCommandHandler : ICommandHandler
{
    private readonly IWorkerConfigurationValidator  _validator;
    private readonly IWorkerConfigurationRepo _repo;
    private readonly IScheduler _scheduler;
    private readonly ITransactionProvider _transactionProvider;

    public UpdateCommandHandler(IWorkerConfigurationValidator validator, IWorkerConfigurationRepo repo, IScheduler scheduler, ITransactionProvider transactionProvider)
    {
        _validator = validator;
        _repo = repo;
        _scheduler = scheduler;
        _transactionProvider = transactionProvider;
    }

    public bool CanHandle(Command command)
    {
        return command.Name.Equals("update", StringComparison.InvariantCultureIgnoreCase);
    }

    public async Task<string> Handle(Command command, CancellationToken token)
    {
        if (command.WorkerId.IsEmpty())
        {
            return "WorkerId is required for Update command";
        }

        var config = await _repo.Get(command.WorkerId, token);
        if (config is null)
        {
            return $"Worker with Id {command.WorkerId} not found";
        }
        
        var updateCommandParseResult = UpdateCommand.Parse(command);

        if (!updateCommandParseResult.IsSuccess)
        {
            return updateCommandParseResult.Error;
        }

        var updateCommand = updateCommandParseResult.Entity!;

        config = new WorkerConfiguration(
            config.Id,
            config.ProviderInfo with
            {
                Target = updateCommand.ProviderTarget ?? config.ProviderInfo.Target
            },
            updateCommand.Schedule ?? config.Schedule,
            updateCommand.Filter ?? config.Filter,
            config.ClientInfo with
            {
                Target = updateCommand.ClientTarget ?? config.ClientInfo.Target
            },
            updateCommand.Template ?? config.Template
        );

        var validationResult = _validator.Validate(config);

        if (!validationResult.IsSuccess)
        {
            return validationResult.Error;
        }
        
        await using var transacton = await _transactionProvider.CreateTransaction(token);
        
        await _repo.Update(config, token);

        await transacton.Commit(token);
        
        await _scheduler.Schedule(config.Schedule, config.Id);
        return $"Worker {config.Id} has been updated";
    }
}