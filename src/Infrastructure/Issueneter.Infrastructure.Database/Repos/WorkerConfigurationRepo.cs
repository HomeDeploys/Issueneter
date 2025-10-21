using Issueneter.Domain.Interfaces.Repos;
using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;
using Issueneter.Infrastructure.Database.Connection;
using Issueneter.Infrastructure.Database.Repos.Dto;

namespace Issueneter.Infrastructure.Database.Repos;

internal class WorkerConfigurationRepo : IWorkerConfigurationRepo
{
    private const string TableName = "worker_configuration";
    
    private readonly DbConnectionFactory _connectionFactory;

    public WorkerConfigurationRepo(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<WorkerConfiguration?> Get(WorkerId workerId, CancellationToken token)
    {
        var parameters = new
        {
            WorkerId = workerId.Value
        };
        
        const string query = $"SELECT * FROM worker_configuration WHERE worker_id = @{nameof(parameters.WorkerId)}";

        await using var connection = await _connectionFactory.GetConnection(token);
        var dto = await connection.QuerySingleOrDefault<WorkerConfigurationDto>(query, parameters);
        return dto?.ToDomain();
    }

    public async Task<WorkerId> Create(WorkerConfiguration configuration, CancellationToken token)
    {
        var dto = WorkerConfigurationDto.FromDomain(configuration);

        const string query = $"""
          INSERT INTO worker_configuration(
            provider_type,
            provider_target,
            schedule,
            filter,                               
            client_type,                               
            client_target,
            template
          ) VALUES (
            @{nameof(dto.ProviderType)},
            @{nameof(dto.ProviderTarget)},
            @{nameof(dto.Schedule)},
            @{nameof(dto.Filter)},
            @{nameof(dto.ClientType)},
            @{nameof(dto.ClientTarget)},
            @{nameof(dto.Template)}
          ) RETURNING worker_id;
          """;

        await using var connection = await _connectionFactory.GetConnection(token);
        var id = await connection.QuerySingleOrDefault<long>(query, dto);
        return new WorkerId(id);
    }
}