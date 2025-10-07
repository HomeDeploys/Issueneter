using System.Data.Common;
using Issueneter.Domain.Interfaces.Connection;

namespace Issueneter.Infrastructure.Database.Connection;

public class Transaction : ITransaction
{
    private readonly DbConnection _conn;
    private readonly DbTransaction _tran;
    private readonly Action _onClose;

    private Transaction(DbConnection conn, DbTransaction tran, Action onClose)
    {
        (_conn, _tran, _onClose) = (conn, tran, onClose);
        Connection = new TransactionalConnection(tran);
    }

    internal IConnection Connection { get; }
    public Task Commit(CancellationToken token) => _tran.CommitAsync(token);
    public Task Rollback(CancellationToken token) => _tran.RollbackAsync(token);

    public async ValueTask DisposeAsync()
    {
        _onClose();
        await _tran.DisposeAsync();
        await _conn.DisposeAsync();
    }

    public static async ValueTask<Transaction> Create(DbConnection conn, Action onClose, CancellationToken token)
    {
        var tr = await conn.BeginTransactionAsync(token);
        return new Transaction(conn, tr, onClose);
    }
}