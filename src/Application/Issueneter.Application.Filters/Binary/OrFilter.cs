using Issueneter.Domain.Interfaces.Filters;
using Issueneter.Domain.Models;

namespace Issueneter.Application.Parser.Binary;

public class OrFilter : IFilter
{
    private readonly IReadOnlyCollection<IFilter> _filters;
    
    public OrFilter(IReadOnlyCollection<IFilter> filters)
    {
        _filters = filters;
    }

    public bool IsValid(Entity entity)
    {
        return _filters.All(f => f.IsValid(entity));
    }

    public bool IsApplicable(Entity entity)
    {
        return _filters.Any(f => f.IsApplicable(entity));
    }
}