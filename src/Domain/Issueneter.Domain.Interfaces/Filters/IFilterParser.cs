using Issueneter.Domain.ValueObjects;

namespace Issueneter.Domain.Interfaces.Filters;

public interface IFilterParser
{
    ParseResult<IFilter> Parse(string filter);
}