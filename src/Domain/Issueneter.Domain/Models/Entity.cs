using System.Reflection;

namespace Issueneter.Domain.Models;

public abstract class Entity
{
    public bool HasProperty(string propertyName)
    {
        var type = GetType();
        return type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance) is not null;
    }

    public bool IsCastable<T>(string propertyName)
    {
        var type = GetType();

        var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

        if (property is null)
        {
            return false;
        }
        
        var propertyType = property.PropertyType;
        var targetType = typeof(T);
        
        return targetType.IsAssignableFrom(propertyType);
    }

    public T? GetProperty<T>(string propertyName)
    {
        var type = this.GetType();

        var prop = type.GetProperty(propertyName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (prop == null)
            throw new ArgumentException($"Property '{propertyName}' not found in {type.Name}.");

        if (!prop.CanRead)
            throw new InvalidOperationException($"Property '{propertyName}' is write-only.");

        var value = prop.GetValue(this);

        return value switch
        {
            null when default(T) != null => throw new InvalidCastException(
                $"Property '{propertyName}' is null and cannot be cast to non-nullable {typeof(T).Name}."),
            null => default,
            T castValue => castValue,
            _ => throw new InvalidCastException(
                $"Cannot cast property '{propertyName}' of type {value.GetType().Name} to {typeof(T).Name}.")
        };
    }
}