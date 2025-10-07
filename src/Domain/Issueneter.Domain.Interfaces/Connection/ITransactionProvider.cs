namespace Issueneter.Domain.Interfaces.Connection;

public interface ITransactionProvider
{
    Task<ITransaction> CreateTransaction(CancellationToken token);
}