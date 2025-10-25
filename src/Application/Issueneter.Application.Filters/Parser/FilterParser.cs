using Antlr4.Runtime;
using Issueneter.Application.Parser.Unary;
using Issueneter.Domain.Interfaces.Filters;
using Issueneter.Domain.ValueObjects;

namespace Issueneter.Application.Parser.Parser;

internal class FilterParser : IFilterParser
{
    public ParseResult<IFilter> Parse(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return ParseResult<IFilter>.Success(new EmptyFilter());
        }
        
        var inputStream = new AntlrInputStream(filter.Trim());
        var lexer = new QueryLexer(inputStream);
        var tokens = new CommonTokenStream(lexer);
        var parser = new QueryParser(tokens);
        var errorListener = new FilterErrorListener();
        parser.AddErrorListener(errorListener);
        var query = parser.query();
        if (errorListener.HasErrors(out var errorMessage))
        {
            return ParseResult<IFilter>.Fail(errorMessage);
        }
        
        var builder = new FilterBuilder();
        var filterObject = query.Accept(builder);
        return ParseResult<IFilter>.Success(filterObject);
    }
}