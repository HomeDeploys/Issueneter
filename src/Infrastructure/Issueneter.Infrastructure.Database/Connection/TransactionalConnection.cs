using System.Data.Common;

namespace Issueneter.Infrastructure.Database.Connection;

internal class TransactionalConnection : AbstractConnection
{
    private readonly DbTransaction _transaction;

    public TransactionalConnection(DbTransaction transaction)
    {
        _transaction = transaction;
    }

    protected override DbConnection Connection => _transaction.Connection!;

    public override ValueTask DisposeAsync() => ValueTask.CompletedTask;
}