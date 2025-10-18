using Issueneter.Common.Extensions;
using Issueneter.Domain.Interfaces.Repos;
using Issueneter.Domain.ValueObjects;
using Issueneter.Infrastructure.Database.Connection;

namespace Issueneter.Infrastructure.Database.Repos;

internal class ProviderSnapshotRepo : IProviderSnapshotRepo
{
    private const string TableName = "provider_snapshot";

    private readonly DbConnectionFactory _connectionFactory;

    public ProviderSnapshotRepo(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<T?> GetLastSnapshot<T>(WorkerId workerId, CancellationToken token) where T : class
    {
        const string query = $"""
            select data
            from {TableName}
            where worker_id = @{nameof(workerId)} 
        """;

        await using var connection = await _connectionFactory.GetConnection(token);
        var data = await connection.QuerySingleOrDefault<string>(query, new { workerId });
        return data?.DeserializeSnakeCase<T>();
    }

    public async Task UpsertSnapshot<T>(WorkerId workerId, T snapshot, CancellationToken token) where T : class
    {
        var parameters = new
        {
            WorkerId = workerId,
            Data = snapshot.SerializeSnakeCase()
        };

        const string query = $"""
            insert into {TableName}
            values (@{nameof(parameters.WorkerId)}, @{nameof(parameters.Data)})
            on conflict (worker_id) 
            do update set
                data = excluded.data
        """;

        await using var connection = await _connectionFactory.GetConnection(token);
        await connection.Execute(query, parameters);
    }
}