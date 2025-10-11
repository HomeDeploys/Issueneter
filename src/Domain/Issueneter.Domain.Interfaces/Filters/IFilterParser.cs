namespace Issueneter.Domain.Interfaces.Filters;

public interface IFilterParser
{
    IFilter Parse(string filter);
}