using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SubsTracker.DAL;

namespace SubsTracker.IntegrationTests.Configuration.ServiceConfigs;

public static class DatabaseTestConfig
{
    public static IServiceCollection ReplaceDatabase(this IServiceCollection services, string connectionString)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SubsDbContext>));

        if (descriptor is not null)
        {
            services.Remove(descriptor);
        }

        services.AddDbContext<SubsDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        return services;
    }
}
