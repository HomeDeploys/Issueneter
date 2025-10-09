using Issueneter.Domain.Interfaces.Filters;
using Issueneter.Domain.Models;

namespace Issueneter.Domain.Filters.Unary;

public class EqualityFilter<T> : IFilter
{
    private readonly string _propertyName;
    private readonly T _value;

    public EqualityFilter(string propertyName, T value)
    {
        _propertyName = propertyName;
        _value = value;
    }
    
    public bool IsValid(Entity entity)
    {
        return entity.IsCastable<T>(_propertyName);
    }

    public bool IsApplicable(Entity entity)
    {
        return _value?.Equals(entity.GetProperty<T>(_propertyName)) ?? false;
    }
}