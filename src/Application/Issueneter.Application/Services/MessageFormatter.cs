using System.Text.RegularExpressions;
using Issueneter.Domain.Interfaces.Services;
using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;

namespace Issueneter.Application.Services;

internal partial class MessageFormatter : IMessageFormatter
{
    [GeneratedRegex(@"\{([A-Za-z0-9_]+)\}", RegexOptions.Compiled)]
    private static partial Regex PlaceholderRegex();
    
    public ValidationResult Validate(string template, Entity entity)
    {
        if (string.IsNullOrEmpty(template))
        {
            return ValidationResult.Fail("Template is empty");
        }
        
        var mathes = PlaceholderRegex().Matches(template);
        foreach (Match match in mathes)
        {
            var propertyName = match.Groups[1].Value;
            if (!entity.HasProperty(propertyName))
            {
                return ValidationResult.Fail($"Property '{propertyName}' not found");
            }
        }

        return ValidationResult.Success;
    }

    public string Format(string template, Entity entity)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return string.Empty;
        }

        return PlaceholderRegex().Replace(template, match =>
        {
            var propertyName = match.Groups[1].Value;
            return entity.GetProperty(propertyName) ?? string.Empty;
        });
    }
}