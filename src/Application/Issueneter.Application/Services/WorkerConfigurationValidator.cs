using Issueneter.Domain.Interfaces.Filters;
using Issueneter.Domain.Interfaces.Services;
using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;

namespace Issueneter.Application.Services;

internal class WorkerConfigurationValidator : IWorkerConfigurationValidator
{
    private readonly IProviderFactory _providerFactory;
    private readonly IClientFactory _clientFactory;
    private readonly IScheduler _scheduler;
    private readonly IMessageFormatter _messageFormatter;
    private readonly IFilterParser _filterParser;

    public WorkerConfigurationValidator(
        IProviderFactory providerFactory,
        IClientFactory clientFactory,
        IScheduler scheduler, 
        IMessageFormatter messageFormatter, 
        IFilterParser filterParser)
    {
        _providerFactory = providerFactory;
        _clientFactory = clientFactory;
        _scheduler = scheduler;
        _messageFormatter = messageFormatter;
        _filterParser = filterParser;
    }

    public ValidationResult Validate(WorkerConfiguration configuration)
    {
        var provider = _providerFactory.Get(configuration.ProviderInfo.Type);

        if (provider is null)
        {
            return ValidationResult.Fail($"No provider found for {configuration.ProviderInfo.Type}");
        }

        var providerValidationResult = provider.Validate(configuration.ProviderInfo.Target);
        if (!providerValidationResult.IsSuccess)
        {
            return ValidationResult.Fail($"Invalid provider target: {providerValidationResult.Error}");
        }
        
        var client = _clientFactory.Get(configuration.ClientInfo.Type);

        if (client is null)
        {
            return ValidationResult.Fail($"No client found for type: {configuration.ClientInfo.Type}");
        }

        var clientValidationResult = client.Validate(configuration.ClientInfo.Target);
        if (!clientValidationResult.IsSuccess)
        {
            return ValidationResult.Fail($"Invalid client target: {clientValidationResult.Error}");
        }
        
        var scheduleValidationResult = _scheduler.Validate(configuration.Schedule);

        if (!scheduleValidationResult.IsSuccess)
        {
            return ValidationResult.Fail($"Invalid schedule: {scheduleValidationResult.Error}");
        }

        var entity = provider.GetSample();

        var filterParseResult = _filterParser.Parse(configuration.Filter);

        if (!filterParseResult.IsSuccess)
        {
            return ValidationResult.Fail($"Invalid filter format: {filterParseResult.Error}");
        }

        var filter = filterParseResult.Entity!;
        if (!filter.IsValid(entity))
        {
            return ValidationResult.Fail("Filter is not valid for this type of entity");
        }

        var messageValidationResult = _messageFormatter.Validate(configuration.Template, entity);
        if (!messageValidationResult.IsSuccess)
        {
            return ValidationResult.Fail($"Invalid template: {messageValidationResult.Error}");
        }
        
        return ValidationResult.Success;
    }
}