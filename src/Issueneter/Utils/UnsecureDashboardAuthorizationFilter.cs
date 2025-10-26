using Hangfire.Dashboard;

namespace Issueneter.Utils;

public class UnsecureDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}