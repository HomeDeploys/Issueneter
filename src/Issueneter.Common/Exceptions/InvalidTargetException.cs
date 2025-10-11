namespace Issueneter.Common.Exceptions;

public class InvalidTargetException : IssueneterException
{
    public InvalidTargetException() { }
    public InvalidTargetException(string message) : base(message) { }
    public InvalidTargetException(string message, Exception inner) : base(message, inner) { }
}