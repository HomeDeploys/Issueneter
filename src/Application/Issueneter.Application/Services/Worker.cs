using Issueneter.Common.Exceptions;
using Issueneter.Domain.Interfaces.Filters;
using Issueneter.Domain.Interfaces.Repos;
using Issueneter.Domain.Interfaces.Services;
using Issueneter.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Issueneter.Application.Services;

internal class Worker : IWorker
{
    private readonly IProviderFactory _providerFactory;
    private readonly IClientFactory _clientFactory;
    private readonly IFilterParser _parser;
    private readonly IMessageFormatter _formatter;
    private readonly IWorkerConfigurationRepo _configurationRepo;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IProviderFactory providerFactory, 
        IClientFactory clientFactory, 
        IFilterParser parser, 
        IMessageFormatter formatter, 
        IWorkerConfigurationRepo configurationRepo, 
        ILogger<Worker> logger)
    {
        _providerFactory = providerFactory;
        _clientFactory = clientFactory;
        _parser = parser;
        _formatter = formatter;
        _configurationRepo = configurationRepo;
        _logger = logger;
    }

    public async Task Execute(WorkerId workerId, CancellationToken token)
    {
        try
        {
            _logger.LogInformation("Executing worker {workerId}", workerId);
            var config = await _configurationRepo.Get(workerId, token)
                            ?? throw new NotFoundException($"Worker {workerId} not found");
            var provider = _providerFactory.Get(config.ProviderInfo.Type) 
                           ?? throw new NotFoundException($"Provider with type {config.ProviderInfo.Type} not found");
            var client = _clientFactory.Get(config.ClientInfo.Type)
                           ??  throw new NotFoundException($"Client with type {config.ClientInfo.Type} not found");

            var entities = await provider.Fetch(workerId, config.ProviderInfo.Target, token);

            var filterParseResult = _parser.Parse(config.Filter);

            if (!filterParseResult.IsSuccess)
            {
                throw new FilterParseException($"Can't parse filter: {filterParseResult.Error}");
            }

            var filter = filterParseResult.Entity!;
            
            
            var messages = entities
                .Where(e => filter.IsApplicable(e))
                .Select(e => _formatter.Format(config.Template, e))
                .ToArray();

            foreach (var message in messages)
            {
                await client.Send(config.ClientInfo.Target, message, token);
            }
            
            _logger.LogInformation("Finish executing worker {workerId}", workerId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to execute worker {WorkerId}", workerId);
        }
    }
}