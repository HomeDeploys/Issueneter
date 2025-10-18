using Issueneter.Domain.ValueObjects;

namespace Issueneter.Domain.Interfaces.Services;

public interface IScheduler
{
    ValidationResult Validate(string schedule);
    Task Schedule(string schedule, WorkerId workerId);
}