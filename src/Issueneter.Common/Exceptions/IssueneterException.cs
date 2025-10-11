namespace Issueneter.Common.Exceptions;

public class IssueneterException : Exception
{
    public IssueneterException() { }
    public IssueneterException(string message) : base(message) { }
    public IssueneterException(string message, Exception inner) : base(message, inner) { }
}