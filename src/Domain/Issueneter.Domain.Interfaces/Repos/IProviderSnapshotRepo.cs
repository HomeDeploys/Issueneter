namespace Issueneter.Domain.Interfaces.Repos;

public interface IProviderSnapshotRepo
{
    Task<T?> GetLastSnapshot<T>(long workerId, CancellationToken token) where T : class;
    Task UpsertSnapshot<T>(long workerId, T snapshot, CancellationToken token) where T : class;
}