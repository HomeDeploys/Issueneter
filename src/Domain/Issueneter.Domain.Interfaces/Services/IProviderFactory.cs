using Issueneter.Domain.Enums;

namespace Issueneter.Domain.Interfaces.Services;

public interface IProviderFactory
{
    IEntityProvider Get(ProviderType providerType);
}