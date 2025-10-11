using Issueneter.Domain.Enums;
using Issueneter.Domain.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Issueneter.Application.Services;

internal class ClientFactory : IClientFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ClientFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IClient? Get(ClientType clientType)
    {
        var clients = _serviceProvider.GetServices<IClient>();
        return clients.FirstOrDefault(client => client.Type == clientType);
    }
}