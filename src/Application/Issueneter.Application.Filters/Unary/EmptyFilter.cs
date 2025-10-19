using Issueneter.Domain.Interfaces.Filters;
using Issueneter.Domain.Models;

namespace Issueneter.Application.Parser.Unary;

public class EmptyFilter : IFilter
{
    public bool IsValid(Entity entity)
    {
        return true;
    }

    public bool IsApplicable(Entity entity)
    {
        return true;
    }
}