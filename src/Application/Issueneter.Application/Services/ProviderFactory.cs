using Issueneter.Domain.Enums;
using Issueneter.Domain.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Issueneter.Application.Services;

internal class ProviderFactory : IProviderFactory
{
    private readonly IEnumerable<IEntityProvider> _providers;

    public ProviderFactory(IEnumerable<IEntityProvider> providers)
    {
        _providers = providers;
    }

    public IEntityProvider? Get(ProviderType providerType)
    {
        return _providers.FirstOrDefault(p => p.Type == providerType);
    }
}