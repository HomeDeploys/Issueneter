using Antlr4.Runtime;
using Issueneter.Common.Exceptions;
using Issueneter.Domain.Interfaces.Filters;

namespace Issueneter.Application.Parser;

internal class FilterParser : IFilterParser
{
    public IFilter Parse(string filter)
    {
        var inputStream = new AntlrInputStream(filter.Trim());
        var lexer = new QueryLexer(inputStream);
        var tokens = new CommonTokenStream(lexer);
        var parser = new QueryParser(tokens);
        var errorListener = new FilterErrorListener();
        parser.AddErrorListener(errorListener);
        var query = parser.query();
        if (errorListener.HasErrors(out var errorMessage))
        {
            throw new FilterParseException(errorMessage);
        }
        
        var builder = new FilterBuilder();
        return query.Accept(builder);
    }
}