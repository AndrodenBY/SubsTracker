using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace SubsTracker.IntegrationTests.Configuration.WebApplicationFactory;

public static class DatabaseTestConfig
{
    private static readonly InMemoryDatabaseRoot DbRoot = new();

    public static IServiceCollection ReplaceDatabase(this IServiceCollection services)
    {
        var descriptors = services.Where(d => 
            d.ServiceType.Namespace?.Contains("EntityFrameworkCore") == true ||
            d.ServiceType == typeof(SubsDbContext) ||
            d.ImplementationType?.Namespace?.Contains("Npgsql") == true
        ).ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
        
        services.AddDbContext<SubsDbContext>(options =>
        {
            options.UseInMemoryDatabase(DatabaseConstant.InMemoryDbName, DbRoot);
            options.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        });

        return services;
    }
}

