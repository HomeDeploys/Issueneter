using Issueneter.Domain.ValueObjects;

namespace Issueneter.Domain.Models;

public record Command(string Name, WorkerId WorkerId, IReadOnlyDictionary<string, string> Parameters);