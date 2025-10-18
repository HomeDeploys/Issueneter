using Issueneter.Domain.Enums;
using Issueneter.Domain.Interfaces.Services;
using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;

namespace Issueneter.Application.Commands;

internal static class CommandExtensions
{
    public static ParseResult<ProviderInfo> ParseProvider(this Command command, IProviderFactory factory, out IEntityProvider? provider)
    {
        if (!command.Parameters.TryGetValue("ProviderType", out var providerTypeRaw))
        {
            provider = null;
            return ParseResult<ProviderInfo>.Fail("Missing required argument: ProviderType");
        }

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

        if (!command.Parameters.TryGetValue("ProviderTarget", out var providerTarget))
        {
            return ParseResult<ProviderInfo>.Fail("Missing required argument: ProviderTarget");
        }

        var result = provider.Validate(providerTarget);
        if (!result.IsSuccess)
        {
            return ParseResult<ProviderInfo>.Fail($"Invalid provider target: {result.Error}");
        }
        
        return ParseResult<ProviderInfo>.Success(new ProviderInfo(providerType, providerTarget));
    }

    public static ParseResult<ClientInfo> ParseClient(this Command command, IClientFactory factory)
    {
        if (!command.Parameters.TryGetValue("ClientType", out var ClientTypeRaw))
        {
            return ParseResult<ClientInfo>.Fail("Missing required argument: ClientType");
        }

        if (!Enum.TryParse<ClientType>(ClientTypeRaw, out var clientType) ||
            !Enum.IsDefined(clientType))
        {
            return ParseResult<ClientInfo>.Fail($"Invalid Client type: {clientType}");
        }

        var Client = factory.Get(clientType);

        if (Client is null)
        {
            return ParseResult<ClientInfo>.Fail($"No Client found for type: {clientType}");
        }

        if (!command.Parameters.TryGetValue("ClientTarget", out var clientTarget))
        {
            return ParseResult<ClientInfo>.Fail("Missing required argument: ClientTarget");
        }

        var result = Client.Validate(clientTarget);
        if (!result.IsSuccess)
        {
            return ParseResult<ClientInfo>.Fail($"Invalid Client target: {result.Error}");
        }
        
        return ParseResult<ClientInfo>.Success(new ClientInfo(clientType, clientTarget));
    }
}