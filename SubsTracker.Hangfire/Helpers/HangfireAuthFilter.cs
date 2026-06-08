using Hangfire.Dashboard;

namespace SubsTracker.Hangfire.Helpers;

public class HangfireAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}
