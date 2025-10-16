using Issueneter.Domain.Models;

namespace Issueneter.Domain.Interfaces.Repos;

public interface IWorkerRepo
{
    Task<WorkerConfiguration> Get(long workerId, CancellationToken token);
}