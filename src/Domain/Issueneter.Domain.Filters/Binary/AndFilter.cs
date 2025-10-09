using Issueneter.Domain.Interfaces.Filters;
using Issueneter.Domain.Models;

namespace Issueneter.Domain.Filters.Binary;

public class AndFilter : IFilter
{
    private readonly IFilter _left;
    private readonly IFilter _right;
    
    public AndFilter(IFilter left, IFilter right)
    {
        _left = left;
        _right = right;
    }

    public bool IsValid(Entity entity)
    {
        return _left.IsValid(entity) && _right.IsValid(entity);
    }

    public bool IsApplicable(Entity entity)
    {
        return _left.IsApplicable(entity) && _right.IsApplicable(entity);
    }
}