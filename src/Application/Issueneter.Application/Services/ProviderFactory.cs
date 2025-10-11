using Issueneter.Domain.Enums;
using Issueneter.Domain.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Issueneter.Application.Services;

internal class ProviderFactory : IProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEntityProvider? Get(ProviderType providerType)
    {
        var providers = _serviceProvider.GetServices<IEntityProvider>();
        return providers.FirstOrDefault(p => p.Type == providerType);
    }
}