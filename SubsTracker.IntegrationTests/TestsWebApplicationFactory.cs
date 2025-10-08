using Microsoft.AspNetCore.Hosting;

namespace SubsTracker.IntegrationTests;

public class TestsWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTest");

        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            var sources = configBuilder.Sources.ToList();
            configBuilder.Sources.Clear();

            foreach (var source in sources)
            {
                var typeName = source.GetType().FullName ?? string.Empty;
                if (!typeName.Contains("UserSecrets", StringComparison.OrdinalIgnoreCase))
                {
                    configBuilder.Sources.Add(source);
                }
            }
        });

        builder.ConfigureServices(services =>
        {
            var descriptorsToRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<SubsDbContext>) ||
                d.ServiceType == typeof(SubsDbContext) ||
                d.ServiceType.Name.Contains("DatabaseProvider") ||
                d.ServiceType.Name.Contains("DbContextOptions") ||
                d.ServiceType.Name.Contains("IDbContextOptionsConfiguration") ||
                d.ImplementationType?.Namespace?.Contains("Npgsql") == true
            ).ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }
            
            services.AddDbContext<SubsDbContext>(options =>
            {
                options.UseInMemoryDatabase(DatabaseConstant.InMemoryDbName);
            });
        });
    }
}
