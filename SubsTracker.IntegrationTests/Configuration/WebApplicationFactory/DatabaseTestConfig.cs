namespace SubsTracker.IntegrationTests.Configuration.WebApplicationFactory;

public static class DatabaseTestConfig
{
    public static IServiceCollection ReplaceDatabase(this IServiceCollection services, string connectionString)
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
            options.UseNpgsql(connectionString);
        });

        return services;
    }
}
