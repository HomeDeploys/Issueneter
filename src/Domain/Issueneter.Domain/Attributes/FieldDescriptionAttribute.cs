namespace Issueneter.Domain.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class FieldDescriptionAttribute(string description) : Attribute
{
    public string Description { get; } = description;
}
