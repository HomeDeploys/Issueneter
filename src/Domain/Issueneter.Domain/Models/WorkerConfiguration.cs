using Issueneter.Domain.Enums;
using Issueneter.Domain.ValueObjects;

namespace Issueneter.Domain.Models;

public record WorkerConfiguration(
    WorkerId Id,
    ProviderInfo ProviderInfo,
    string Schedule,
    string Filter,
    ClientInfo ClientInfo,
    string Template)
{
    public override string ToString()
    {
        return $"""
                Id: {Id}
                ProviderType: {ProviderInfo.Type}
                ProviderTarget: {ProviderInfo.Target}
                Schedule: {Schedule}
                Filter: {Filter}
                ClientType: {ClientInfo.Type}
                ClientTarget: {ClientInfo.Target}
                Template: {Template}
                """;
    }
}
