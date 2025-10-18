using Issueneter.Domain.Enums;

namespace Issueneter.Domain.Models;

public record ClientInfo(ClientType Type, string Target);