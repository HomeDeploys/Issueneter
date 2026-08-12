using Issueneter.Application.Commands.Models;
using Issueneter.Common.Extensions;
using Issueneter.Domain.Enums;
using Issueneter.Domain.Interfaces.Commands;
using Issueneter.Domain.Interfaces.Connection;
using Issueneter.Domain.Interfaces.Repos;
using Issueneter.Domain.Interfaces.Services;
using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;

namespace Issueneter.Application.Commands.Handlers;

internal class CreateCommandHandler : ICommandHandler
{
    private readonly IWorkerConfigurationValidator _validator;
    private readonly IScheduler _scheduler;
    private readonly IWorkerConfigurationRepo _workerConfigurationRepo;
    private readonly ITransactionProvider _transactionProvider;

    public CreateCommandHandler(
        IWorkerConfigurationValidator validator, 
        IScheduler scheduler, 
        IWorkerConfigurationRepo workerConfigurationRepo, 
        ITransactionProvider transactionProvider)
    {
        _validator = validator;
        _scheduler = scheduler;
        _workerConfigurationRepo = workerConfigurationRepo;
        _transactionProvider = transactionProvider;
    }

    public string Name => "create";

    public string Description => "Creates a new worker that polls a provider on a schedule, filters the fetched entities and sends matching ones to a client.";

    public IReadOnlyCollection<CommandArgumentInfo> Arguments { get; } =
    [
        new("ProviderType", "Provider type, e.g. Github", IsRequired: true),
        new("ProviderTarget", "Provider-specific target, e.g. a GitHub repository in 'owner/repo' format", IsRequired: true),
        new("Schedule", "Cron expression defining how often the worker polls the provider", IsRequired: true),
        new("Filter", "Filter DSL expression applied to fetched entities, e.g. EQUALS(State, open)", IsRequired: true),
        new("ClientType", "Client type, e.g. Telegram", IsRequired: true),
        new("ClientTarget", "Client-specific target, e.g. a Telegram chat id", IsRequired: true),
        new("Template", "Message template used to format matching entities", IsRequired: true)
    ];

    public string Example =>
        """
        create
        ProviderType: Github
        ProviderTarget: owner/repo
        Schedule: 0 */5 * * * *
        Filter: EQUALS(State, open)
        ClientType: Telegram
        ClientTarget: 123
        Template: {Title}
        """;

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

        if (!Enum.TryParseSafe<ProviderType>(createCommand.ProviderType, out var providerType))
        {
            return $"Invalid provider type: {createCommand.ProviderType}";
        }
        
        if (!Enum.TryParseSafe<ClientType>(createCommand.ClientType, out var clientType))
        {
            return $"Invalid client type: {createCommand.ClientType}";
        }

        var config = new WorkerConfiguration(
            WorkerId.Empty,
            new ProviderInfo(providerType, createCommand.ProviderTarget),
            createCommand.Schedule,
            createCommand.Filter,
            new ClientInfo(clientType, createCommand.ClientTarget),
            createCommand.Template);
        
        var configValidationResult = _validator.Validate(config);
        if (!configValidationResult.IsSuccess)
        {
            return configValidationResult.Error;
        }

        await using var transacton = await _transactionProvider.CreateTransaction(token);
        
        var workerId = await _workerConfigurationRepo.Create(config, token);
        config = config with
        {
            Id = workerId
        };

        await transacton.Commit(token);
        
        await _scheduler.Schedule(config.Schedule, workerId);
        return $"Worker {config.Id} has been created";
    }
}