using Issueneter.Domain.Models;

namespace Issueneter.Domain.Interfaces.Filters;

public interface IFilter
{
    bool IsValid(Entity entity);
    bool IsApplicable(Entity entity);
}