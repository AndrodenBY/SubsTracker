namespace SubsTracker.IntegrationTests.Configuration.WebApplicationFactory;

public static class DatabaseTestConfig
{
    public static IServiceCollection ReplaceDatabase(
        this IServiceCollection services,
        string connectionString)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SubsDbContext>));

        if (descriptor != null)
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
