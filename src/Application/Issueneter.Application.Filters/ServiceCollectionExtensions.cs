using Issueneter.Domain.Interfaces.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Issueneter.Application.Parser;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFilters(this IServiceCollection services)
    {
        return services.AddScoped<IFilterParser, FilterParser>();
    }
}