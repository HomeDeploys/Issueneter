using Issueneter.Domain.Models;

namespace Issueneter.Domain.Interfaces.Filters;

public interface IFilter
{
    // TODO: Make more specific validation result (what field is missing/has wrong format)
    bool IsValid(Entity entity);
    bool IsApplicable(Entity entity);
}