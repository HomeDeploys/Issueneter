using Issueneter.Domain.Interfaces.Filters;
using Issueneter.Domain.Models;

namespace Issueneter.Domain.Filters.Unary;

public class ContainsFilter<T> : IFilter
{
    private readonly string _propertyName;
    private readonly T _value;

    public ContainsFilter(string propertyName, T value)
    {
        _propertyName = propertyName;
        _value = value;
    }

    public bool IsValid(Entity entity)
    {
        return entity.IsCastable<IEnumerable<T>>(_propertyName);
    }

    public bool IsApplicable(Entity entity)
    {
        return entity.GetProperty<IEnumerable<T>>(_propertyName)?.Contains(_value) ?? false;
    }
}