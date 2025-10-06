namespace SubsTracker.IntegrationTests;

public class TestsWebApplicationFactory : WebApplicationFactory<Program>
{
    internal readonly WebApplicationFactory<Program> WebHost;
    
    public TestsWebApplicationFactory()
    {
        WebHost = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                var descriptorsToRemove = services.Where(descriptor =>
                    descriptor.ServiceType == typeof(DbContextOptions<SubsDbContext>) ||
                    descriptor.ImplementationType?.Namespace?.Contains("Npgsql") == true
                ).ToList();
    
                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }
    
                services.AddDbContext<SubsDbContext>(options =>
                {
                    options.UseInMemoryDatabase(DatabaseConstant.InMemoryDbName);
                });
            }));
    }
}
