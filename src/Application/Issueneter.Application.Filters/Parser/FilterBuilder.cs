using System.Globalization;
using Issueneter.Application.Parser.Binary;
using Issueneter.Application.Parser.Unary;
using Issueneter.Domain.Interfaces.Filters;

namespace Issueneter.Application.Parser.Parser;

internal class FilterBuilder : QueryBaseVisitor<IFilter>
{
    public override IFilter VisitQuery(QueryParser.QueryContext context)
    {
        return context.expr().Accept(this);
    }

    public override IFilter VisitBinaryOp(QueryParser.BinaryOpContext context)
    {
        var filters = context.expr().Select(e => e.Accept(this)).ToList();

        if (context.AND() is not null)
        {
            return new AndFilter(filters);
        }

        if (context.OR() is not null)
        {
            return new OrFilter(filters);
        }

        throw new ArgumentException($"Undefined binary filter {context.GetText()}");
    }

    public override IFilter VisitUnaryOp(QueryParser.UnaryOpContext context)
    {
        var name = context.nameToken().GetText();
        var valueToken = context.valueToken();

        if (valueToken.DOUBLE() is not null)
        {
            var value = double.Parse(valueToken.DOUBLE().GetText(), CultureInfo.InvariantCulture);
            return GetUnaryFilter(context, name, value);
        }

        if (valueToken.INTEGER() is not null)
        {
            var value = int.Parse(valueToken.INTEGER().GetText(), CultureInfo.InvariantCulture);
            return GetUnaryFilter(context, name, value);
        }

        if (valueToken.STRING() is not null)
        {
            var value = valueToken.STRING().GetText().Trim('"').Replace("\\\"", "\"");
            return GetUnaryFilter(context, name, value);
        }
        
        throw new ArgumentException($"Undefined unary filter value {context.GetText()}");
    }

    private IFilter GetUnaryFilter<T>(QueryParser.UnaryOpContext context, string name, T value)
    {
        if (context.EQUALS() is not null)
        {
            return new EqualityFilter<T>(name, value);
        }

        if (context.CONTAINS() is not null)
        {
            return new ContainsFilter<T>(name, value);
        }

        throw new ArgumentException($"Undefined unary filter type {context.GetText()}");
    }
}