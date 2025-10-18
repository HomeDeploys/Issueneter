using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;

namespace Issueneter.Domain.Interfaces.Services;

public interface IMessageFormatter
{
    ValidationResult Validate(string template, Entity entity);
    string Format(string template, Entity entity);
}