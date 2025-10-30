using Cronos;
using Hangfire;
using Issueneter.Domain.Interfaces.Services;
using Issueneter.Domain.ValueObjects;

namespace Issueneter.Infrastructure.Background.Services;

internal class Scheduler : IScheduler
{
    private readonly IRecurringJobManager _recurringJobManager;

    public Scheduler(IRecurringJobManager recurringJobManager)
    {
        _recurringJobManager = recurringJobManager;
    }

    public ValidationResult Validate(string schedule)
    {
        return CronExpression.TryParse(schedule, out var _)
            ? ValidationResult.Success
            : ValidationResult.Fail($"Invalid cron expression '{schedule}'");
    }

    public Task Schedule(string schedule, WorkerId workerId)
    {
        _recurringJobManager.AddOrUpdate<IWorker>(
            workerId.ToString(), 
            w => w.Execute(workerId, CancellationToken.None),
            () => schedule);

        return Task.CompletedTask;
    }

    public Task Deschedule(WorkerId workerId)
    {
        _recurringJobManager.RemoveIfExists(workerId.ToString());
        
        return Task.CompletedTask;
    }
}