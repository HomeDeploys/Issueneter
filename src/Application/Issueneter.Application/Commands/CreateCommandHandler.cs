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
    private readonly IWorkerRepo _workerRepo;
    private readonly ITransactionProvider _transactionProvider;
    
    public CreateCommandHandler(
        IProviderFactory providerFactory, 
        IClientFactory clientFactory, 
        IScheduler scheduler, 
        IMessageFormatter messageFormatter, 
        IFilterParser filterParser, 
        IWorkerRepo workerRepo, 
        ITransactionProvider transactionProvider)
    {
        _providerFactory = providerFactory;
        _clientFactory = clientFactory;
        _scheduler = scheduler;
        _messageFormatter = messageFormatter;
        _filterParser = filterParser;
        _workerRepo = workerRepo;
        _transactionProvider = transactionProvider;
    }

    public bool CanHandle(Command command)
    {
        return command.Name.Equals("create", StringComparison.InvariantCultureIgnoreCase);
    }

    public async Task<string> Handle(Command command, CancellationToken token)
    {
        // TODO: Maybe change conception: move command parameters parsing to separated step, and keep only validation & logic here
        var providerParseResult = command.ParseProvider(_providerFactory, out var provider);

        if (!providerParseResult.IsSuccess)
        {
            return providerParseResult.Error;
        }

        var clientParseResult = command.ParseClient(_clientFactory);

        if (!clientParseResult.IsSuccess)
        {
            return clientParseResult.Error;
        }

        if (!command.Parameters.TryGetValue("Schedule", out var schedule))
        {
            return "Missing required parameter: Schedule";
        }

        var scheduleValidationResult = _scheduler.Validate(schedule);

        if (!scheduleValidationResult.IsSuccess)
        {
            return $"Invalid schedule: {scheduleValidationResult.Error}";
        }

        var entity = provider!.GetSample();

        if (!command.Parameters.TryGetValue("Filter", out var filterString))
        {
            return "Missing required parameter: Filter";
        }

        var filterParseResult = _filterParser.Parse(filterString);

        if (!filterParseResult.IsSuccess)
        {
            return $"Invalid filter format: {filterParseResult.Error}";
        }

        var filter = filterParseResult.Entity!;
        if (!filter.IsValid(entity))
        {
            return $"Filter is not valid for this type of entity";
        }

        if (!command.Parameters.TryGetValue("Template", out var template))
        {
            return "Missing required parameter: Template";
        }

        var messageValidationResult = _messageFormatter.Validate(template, entity);
        if (!messageValidationResult.IsSuccess)
        {
            return $"Invalid template: {template}";
        }

        var config = new WorkerConfiguration(
            WorkerId.Empty,
            providerParseResult.Entity!,
            schedule,
            filterString,
            clientParseResult.Entity!,
            template);

        await using var transacton = await _transactionProvider.CreateTransaction(token);
        
        var workerId = await _workerRepo.Create(config, token);
        config = config with
        {
            Id = workerId
        };

        await _scheduler.Schedule(schedule, workerId);
        await transacton.Commit(token);
        
        return $"Worker {config.Id} has been created";
    }
}