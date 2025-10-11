namespace Issueneter.Domain.Interfaces.Repos;

public interface IProviderSnapshotRepo
{
    Task<T?> GetLastSnapshot<T>(long scheduleId) where T : class;
    Task UpsertSnapshot<T>(long scheduleId, T snapshot) where T : class;
}