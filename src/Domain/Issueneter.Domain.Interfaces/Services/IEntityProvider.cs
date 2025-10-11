using Issueneter.Domain.Enums;
using Issueneter.Domain.Models;

namespace Issueneter.Domain.Interfaces.Services;

public interface IEntityProvider
{
    ProviderType Type { get; }
    bool Validate(string target);
    Task<IReadOnlyCollection<Entity>> Fetch(long scheduleId, string target, CancellationToken token);
}