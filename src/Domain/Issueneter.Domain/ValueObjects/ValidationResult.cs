namespace Issueneter.Domain.ValueObjects;

public record ValidationResult(bool IsSuccess, string Error)
{
    public static ValidationResult Success => new(true, string.Empty);
    public static ValidationResult Fail(string error) => new(false, error);
}