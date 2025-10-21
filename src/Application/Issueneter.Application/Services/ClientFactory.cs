using Issueneter.Domain.Enums;
using Issueneter.Domain.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Issueneter.Application.Services;

internal class ClientFactory : IClientFactory
{
    private readonly IEnumerable<IClient> _clients;

    public ClientFactory(IEnumerable<IClient> clients)
    {
        _clients = clients;
    }

    public IClient? Get(ClientType clientType)
    {
        return _clients.FirstOrDefault(client => client.Type == clientType);
    }
}