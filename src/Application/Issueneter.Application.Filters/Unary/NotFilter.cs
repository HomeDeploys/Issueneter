using Issueneter.Domain.Interfaces.Filters;
using Issueneter.Domain.Models;

namespace Issueneter.Application.Parser.Unary;

public class NotFilter : IFilter
{
    private readonly IFilter _filter;
    
    public NotFilter(IFilter filter)
    {
        _filter = filter;
    }

    public bool IsValid(Entity entity)
    {
        return _filter.IsValid(entity);
    }

    public bool IsApplicable(Entity entity)
    {
        return !_filter.IsApplicable(entity);
    }
}