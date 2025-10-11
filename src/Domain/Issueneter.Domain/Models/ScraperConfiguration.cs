using Issueneter.Domain.Enums;

namespace Issueneter.Domain.Models;

public record ScraperConfiguration(
    ProviderType ProviderType,
    string ProviderTarget,
    string Schedule,
    ClientType ClientType,
    IReadOnlyCollection<string> ClientTarget,
    string Template);
