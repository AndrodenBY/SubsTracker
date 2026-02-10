using Auth0.AuthenticationApi;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SubsTracker.API.Auth0;

namespace SubsTracker.IntegrationTests;

public class TestsWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly InMemoryDatabaseRoot DbRoot = new();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "IntegrationTest");
        
        builder.UseEnvironment("IntegrationTest");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.Sources.Clear();
            
            var testAuth0Config = new Dictionary<string, string?>
            {
                ["Auth0:Domain"] = "fake-ci.auth0.com",
                ["Auth0:ClientId"] = "fake-client-id",
                ["Auth0:ClientSecret"] = "fake-client-secret",
                ["Auth0:Audience"] = "fake-audience",
                ["Auth0:Authority"] = "https://fake-ci.auth0.com/",
                ["Auth0:ManagementApiUrl"] = "https://fake-ci.auth0.com/api/v2/"
            };

            config.AddInMemoryCollection(testAuth0Config);
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

            foreach (var descriptor in descriptorsToRemove) services.Remove(descriptor);

            services.AddDbContext<SubsDbContext>(options =>
            {
                options.UseInMemoryDatabase(DatabaseConstant.InMemoryDbName, DbRoot);
            });

            services.RemoveAll<AuthenticationApiClient>();
            services.AddSingleton(new AuthenticationApiClient(new Uri("https://fake-ci.auth0.com/")));

            services.RemoveAll<IAuth0Service>();
            services.AddSingleton<IAuth0Service, FakeAuth0Service>();

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "TestAuthScheme";
                    options.DefaultChallengeScheme = "TestAuthScheme";
                })
                .AddScheme<AuthenticationSchemeOptions, TestsAuthHandler>(
                    "TestAuthScheme", _ => { });
            
            services.RemoveAll<JwtBearerOptions>();
            
            services.AddMassTransitTestHarness();

        });
    }
}
