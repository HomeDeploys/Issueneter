using Issueneter.Domain.Enums;
using Issueneter.Domain.Interfaces.Services;
using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;

namespace Issueneter.Application.Commands;

internal static class CommandHelpers
{
    public static ParseResult<ProviderInfo> ParseProvider(string providerTypeRaw, string providerTarget, IProviderFactory factory, out IEntityProvider? provider)
    {

        if (!Enum.TryParse<ProviderType>(providerTypeRaw, out var providerType) ||
            !Enum.IsDefined(providerType))
        {
            provider = null;
            return ParseResult<ProviderInfo>.Fail($"Invalid provider type: {providerType}");
        }

        provider = factory.Get(providerType);

        if (provider is null)
        {
            return ParseResult<ProviderInfo>.Fail($"No provider found for type: {providerType}");
        }

        var result = provider.Validate(providerTarget);
        if (!result.IsSuccess)
        {
            return ParseResult<ProviderInfo>.Fail($"Invalid provider target: {result.Error}");
        }
        
        return ParseResult<ProviderInfo>.Success(new ProviderInfo(providerType, providerTarget));
    }

    public static ParseResult<ClientInfo> ParseClient(string clientTypeRaw, string clientTarget, IClientFactory factory)
    {
        if (!Enum.TryParse<ClientType>(clientTypeRaw, out var clientType) ||
            !Enum.IsDefined(clientType))
        {
            return ParseResult<ClientInfo>.Fail($"Invalid Client type: {clientType}");
        }

        var client = factory.Get(clientType);

        if (client is null)
        {
            return ParseResult<ClientInfo>.Fail($"No Client found for type: {clientType}");
        }

        var result = client.Validate(clientTarget);
        if (!result.IsSuccess)
        {
            return ParseResult<ClientInfo>.Fail($"Invalid Client target: {result.Error}");
        }
        
        return ParseResult<ClientInfo>.Success(new ClientInfo(clientType, clientTarget));
    }
}