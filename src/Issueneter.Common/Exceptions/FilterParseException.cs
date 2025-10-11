namespace Issueneter.Common.Exceptions;

public class FilterParseException : Exception
{
    public FilterParseException() { }
    public FilterParseException(string message) : base(message) { }
    public FilterParseException(string message, Exception inner) : base(message, inner) { }
}