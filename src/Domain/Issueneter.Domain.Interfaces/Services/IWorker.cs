using Issueneter.Domain.ValueObjects;

namespace Issueneter.Domain.Interfaces.Services;

public interface IWorker
{
    Task Execute(WorkerId workerId, CancellationToken token);
}