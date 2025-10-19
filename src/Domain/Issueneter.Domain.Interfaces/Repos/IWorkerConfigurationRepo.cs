using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;

namespace Issueneter.Domain.Interfaces.Repos;

public interface IWorkerConfigurationRepo
{
    Task<WorkerConfiguration> Get(WorkerId workerId, CancellationToken token);
    Task<WorkerId> Create(WorkerConfiguration configurationInfo, CancellationToken token);
}