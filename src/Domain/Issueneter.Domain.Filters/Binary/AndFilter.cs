using Issueneter.Domain.Interfaces.Filters;
using Issueneter.Domain.Models;

namespace Issueneter.Domain.Filters.Binary;

public class AndFilter : IFilter
{
    private readonly IReadOnlyCollection<IFilter> _filters;
    
    public AndFilter(IReadOnlyCollection<IFilter> filters)
    {
        _filters = filters;
    }

    public bool IsValid(Entity entity)
    {
        return _filters.All(f => f.IsValid(entity));
    }

    public bool IsApplicable(Entity entity)
    {
        return _filters.All(f => f.IsApplicable(entity));
    }
}