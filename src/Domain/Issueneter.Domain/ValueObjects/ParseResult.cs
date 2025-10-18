namespace Issueneter.Domain.ValueObjects;

public record ParseResult<T>(T? Entity, string Error) where T : class
{
    public static ParseResult<T> Success(T entity) => new(entity, string.Empty);
    public static ParseResult<T> Fail(string error) => new(null, error);
    
    public bool IsSuccess => Entity is not null;
}