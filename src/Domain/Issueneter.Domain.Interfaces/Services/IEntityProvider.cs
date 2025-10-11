using Issueneter.Domain.Enums;
using Issueneter.Domain.Models;

namespace Issueneter.Domain.Interfaces.Services;

public interface IEntityProvider
{
    static ProviderType Type { get; }
    bool Validate(string target);
    Task<IReadOnlyCollection<Entity>> Fetch(string target, CancellationToken token);
}