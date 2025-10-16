using Issueneter.Domain.Enums;

namespace Issueneter.Domain.Models;

public record WorkerConfiguration(
    long Id,
    ProviderType ProviderType,
    string ProviderTarget,
    string Schedule,
    string Filter,
    ClientType ClientType,
    IReadOnlyCollection<string> ClientTarget,
    string Template);
