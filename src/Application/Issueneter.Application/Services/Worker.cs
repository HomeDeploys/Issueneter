using Issueneter.Common.Exceptions;
using Issueneter.Domain.Interfaces.Filters;
using Issueneter.Domain.Interfaces.Repos;
using Issueneter.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Issueneter.Application.Services;

internal class Worker : IWorker
{
    private readonly IProviderFactory _providerFactory;
    private readonly IClientFactory _clientFactory;
    private readonly IFilterParser _parser;
    private readonly IMessageFormatter _formatter;
    private readonly IWorkerRepo _repo;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IProviderFactory providerFactory, 
        IClientFactory clientFactory, 
        IFilterParser parser, 
        IMessageFormatter formatter, 
        IWorkerRepo repo, 
        ILogger<Worker> logger)
    {
        _providerFactory = providerFactory;
        _clientFactory = clientFactory;
        _parser = parser;
        _formatter = formatter;
        _repo = repo;
        _logger = logger;
    }

    public async Task Execute(long workerId, CancellationToken token)
    {
        try
        {
            var config = await _repo.Get(workerId, token);
            var provider = _providerFactory.Get(config.ProviderType) 
                           ?? throw new NotFoundException($"Provider with type {config.ProviderType} not found");
            var client = _clientFactory.Get(config.ClientType)
                           ??  throw new NotFoundException($"Client with type {config.ClientType} not found");

            var entities = await provider.Fetch(workerId, config.ProviderTarget, token);

            var filter = _parser.Parse(config.Filter);

            var messages = entities
                .Where(e => filter.IsApplicable(e))
                .Select(e => _formatter.Format(config.Template, e))
                .ToArray();

            foreach (var target in config.ClientTarget)
            {
                foreach (var message in messages)
                {
                    await client.Send(target, message, token);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to execute worker {WorkerId}", workerId);
        }
    }
}