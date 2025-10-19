using Issueneter.Application.Commands.Models;
using Issueneter.Domain.Interfaces.Commands;
using Issueneter.Domain.Interfaces.Connection;
using Issueneter.Domain.Interfaces.Filters;
using Issueneter.Domain.Interfaces.Repos;
using Issueneter.Domain.Interfaces.Services;
using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;

namespace Issueneter.Application.Commands;

internal class CreateCommandHandler : ICommandHandler
{
    private readonly IProviderFactory _providerFactory;
    private readonly IClientFactory _clientFactory;
    private readonly IScheduler _scheduler;
    private readonly IFilterParser _filterParser;
    private readonly IMessageFormatter _messageFormatter;
    private readonly IWorkerConfigurationRepo _workerConfigurationRepo;
    private readonly ITransactionProvider _transactionProvider;
    
    public CreateCommandHandler(
        IProviderFactory providerFactory, 
        IClientFactory clientFactory, 
        IScheduler scheduler, 
        IMessageFormatter messageFormatter, 
        IFilterParser filterParser, 
        IWorkerConfigurationRepo workerConfigurationRepo, 
        ITransactionProvider transactionProvider)
    {
        _providerFactory = providerFactory;
        _clientFactory = clientFactory;
        _scheduler = scheduler;
        _messageFormatter = messageFormatter;
        _filterParser = filterParser;
        _workerConfigurationRepo = workerConfigurationRepo;
        _transactionProvider = transactionProvider;
    }

    public bool CanHandle(Command command)
    {
        return command.Name.Equals("create", StringComparison.InvariantCultureIgnoreCase);
    }

    public async Task<string> Handle(Command command, CancellationToken token)
    {
        var createCommandParseResult = CreateCommand.Parse(command);

        if (!createCommandParseResult.IsSuccess)
        {
            return createCommandParseResult.Error;
        }

        var createCommand = createCommandParseResult.Entity!;

        var providerParseResult = createCommand.ParseProvider(_providerFactory, out var provider);

        if (!providerParseResult.IsSuccess)
        {
            return providerParseResult.Error;
        }

        var clientParseResult = createCommand.ParseClient(_clientFactory);

        if (!clientParseResult.IsSuccess)
        {
            return clientParseResult.Error;
        }

        var scheduleValidationResult = _scheduler.Validate(createCommand.Schedule);

        if (!scheduleValidationResult.IsSuccess)
        {
            return $"Invalid schedule: {scheduleValidationResult.Error}";
        }

        var entity = provider!.GetSample();

        var filterParseResult = _filterParser.Parse(createCommand.Filter);

        if (!filterParseResult.IsSuccess)
        {
            return $"Invalid filter format: {filterParseResult.Error}";
        }

        var filter = filterParseResult.Entity!;
        if (!filter.IsValid(entity))
        {
            return "Filter is not valid for this type of entity";
        }

        var messageValidationResult = _messageFormatter.Validate(createCommand.Template, entity);
        if (!messageValidationResult.IsSuccess)
        {
            return $"Invalid template: {createCommand.Template}";
        }

        var config = new WorkerConfiguration(
            WorkerId.Empty,
            providerParseResult.Entity!,
            createCommand.Schedule,
            createCommand.Filter,
            clientParseResult.Entity!,
            createCommand.Template);

        await using var transacton = await _transactionProvider.CreateTransaction(token);
        
        var workerId = await _workerConfigurationRepo.Create(config, token);
        config = config with
        {
            Id = workerId
        };

        await _scheduler.Schedule(config.Schedule, workerId);
        await transacton.Commit(token);
        
        return $"Worker {config.Id} has been created";
    }
}