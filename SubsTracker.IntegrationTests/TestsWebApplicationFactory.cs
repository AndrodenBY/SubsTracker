namespace SubsTracker.IntegrationTests;

public class TestsWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTest");

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

            foreach (var descriptor in descriptorsToRemove) services.Remove(descriptor);

            services.AddDbContext<SubsDbContext>(options =>
            {
                options.UseInMemoryDatabase(DatabaseConstant.InMemoryDbName);
            });

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = "https://test.authority";
                options.Audience = "test.audience";
            });

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "TestAuthScheme";
                    options.DefaultChallengeScheme = "TestAuthScheme";
                })
                .AddScheme<AuthenticationSchemeOptions, TestsAuthHandler>(
                    "TestAuthScheme", options => { });

            services.AddMassTransitTestHarness();
        });
    }
}
