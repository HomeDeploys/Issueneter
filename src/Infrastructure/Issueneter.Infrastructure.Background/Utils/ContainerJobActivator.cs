using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace Issueneter.Infrastructure.Background.Utils;

public class ContainerJobActivator : JobActivator
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ContainerJobActivator(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public override object ActivateJob(Type jobType)
    {
        using var scope = _scopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService(jobType);
    }
}