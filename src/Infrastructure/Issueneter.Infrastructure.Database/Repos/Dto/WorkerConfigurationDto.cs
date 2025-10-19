using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;

namespace Issueneter.Infrastructure.Database.Repos.Dto;

internal class WorkerConfigurationDto
{
    public required long WorkerId { get; set; }
    public required short ProviderType { get; set; }
    public required string ProviderTarget { get; set; }
    public required string Schedule { get; set; }
    public required string Filter { get; set; }
    public required short ClientType { get; set; }
    public required string ClientTarget { get; set; }
    public required string Template { get; set; }

    public static WorkerConfigurationDto FromDomain(WorkerConfiguration configuration)
    {
        return new WorkerConfigurationDto()
        {
            WorkerId = configuration.Id.Value,
            ProviderType = (short)configuration.ProviderInfo.Type,
            ProviderTarget = configuration.ProviderInfo.Target,
            Schedule = configuration.Schedule,
            Filter = configuration.Filter,
            ClientType = (short)configuration.ClientInfo.Type,
            ClientTarget = configuration.ClientInfo.Target,
            Template = configuration.Template
        };
    }
    
    public WorkerConfiguration ToDomain()
    {
        return new WorkerConfiguration(
            new WorkerId(WorkerId),
            new ProviderInfo(
                (Domain.Enums.ProviderType)ProviderType,
                ProviderTarget),
            Schedule,
            Filter,
            new ClientInfo(
                (Domain.Enums.ClientType)ClientType,
                ClientTarget),
            Template);
    }
}