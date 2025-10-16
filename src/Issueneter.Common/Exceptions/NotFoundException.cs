namespace Issueneter.Common.Exceptions;

public class NotFoundException : IssueneterException
{
    public NotFoundException() { }
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string message, Exception inner) : base(message, inner) { }
}