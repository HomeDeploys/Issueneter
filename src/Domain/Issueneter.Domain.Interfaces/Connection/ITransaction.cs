namespace Issueneter.Domain.Interfaces.Connection;

public interface ITransaction : IAsyncDisposable
{
    Task Commit(CancellationToken token);
    Task Rollback(CancellationToken token);
}