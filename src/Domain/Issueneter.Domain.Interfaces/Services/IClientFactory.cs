using Issueneter.Domain.Enums;

namespace Issueneter.Domain.Interfaces.Services;

public interface IClientFactory
{
    IClient Get(ClientType clientType);
}