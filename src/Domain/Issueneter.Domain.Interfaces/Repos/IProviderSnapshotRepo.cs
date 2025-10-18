using Issueneter.Domain.ValueObjects;

namespace Issueneter.Domain.Interfaces.Repos;

public interface IProviderSnapshotRepo
{
    Task<T?> GetLastSnapshot<T>(WorkerId workerId, CancellationToken token) where T : class;
    Task UpsertSnapshot<T>(WorkerId workerId, T snapshot, CancellationToken token) where T : class;
}