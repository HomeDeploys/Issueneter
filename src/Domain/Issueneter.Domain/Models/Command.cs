namespace Issueneter.Domain.Models;

public record Command(string Name, IReadOnlyDictionary<string, string> Parameters);