namespace Issueneter.Domain.Interfaces.Services;

public interface IWorker
{
    Task Execute(long workerId, CancellationToken token);
}