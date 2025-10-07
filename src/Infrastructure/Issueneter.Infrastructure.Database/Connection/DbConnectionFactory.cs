using Issueneter.Domain.Interfaces.Connection;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Issueneter.Infrastructure.Database.Connection;

internal class DbConnectionFactory : ITransactionProvider
{
    private readonly NpgsqlDataSource _dataSource;
    private Transaction? _transaction;

    public DbConnectionFactory(IOptions<DbConnectionFactoryConfiguration> configuration)
    {
        _dataSource = NpgsqlDataSource.Create(configuration.Value.ConnectionString);
    }

    public async Task<IConnection> GetConnection(CancellationToken token)
    {
        if (_transaction is not null)
        {
            return _transaction.Connection;
        }
        
        var connection = await _dataSource.OpenConnectionAsync(token);
        return new DefaultConnection(connection);
    }

    public async Task<ITransaction> CreateTransaction(CancellationToken token)
    {
        if (_transaction is not null)
        {
            return _transaction;
        }
        
        var connection = await _dataSource.OpenConnectionAsync(token);
        _transaction = await Transaction.Create(connection, () => _transaction = null, token);
        return _transaction;
    }
}