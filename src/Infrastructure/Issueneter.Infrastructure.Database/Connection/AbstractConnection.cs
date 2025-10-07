using System.Data.Common;
using Dapper;

namespace Issueneter.Infrastructure.Database.Connection;

internal abstract class AbstractConnection : IConnection
{
    protected abstract DbConnection Connection { get; }

    public Task Execute(string sql, object? param = null)
        => Connection.ExecuteAsync(sql, param);
    
    public Task<T?> QuerySingleOrDefault<T>(string sql, object? param = null)
        => Connection.QuerySingleOrDefaultAsync<T>(sql, param);

    public abstract ValueTask DisposeAsync();
}