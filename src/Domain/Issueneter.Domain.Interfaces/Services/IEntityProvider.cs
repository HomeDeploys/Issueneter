using Issueneter.Domain.Enums;
using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;

namespace Issueneter.Domain.Interfaces.Services;

public interface IEntityProvider
{
    ProviderType Type { get; }
    ValidationResult Validate(string target);
    Task<IReadOnlyCollection<Entity>> Fetch(WorkerId workerId, string target, CancellationToken token);
    Entity GetSample();
}