using Issueneter.Domain.Enums;

namespace Issueneter.Domain.Models;

public record ProviderInfo(ProviderType Type, string Target);
