namespace Issueneter.Domain.Models;

public record ValidationResult(bool IsSuccess, IReadOnlyCollection<string> Errors)
{
    public static ValidationResult Success => new ValidationResult(true, []);
    public static ValidationResult Fail(IReadOnlyCollection<string> errors) => new ValidationResult(false, errors);
}