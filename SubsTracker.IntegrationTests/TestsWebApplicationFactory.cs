using Auth0.AuthenticationApi;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SubsTracker.IntegrationTests;

public class TestsWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly InMemoryDatabaseRoot DbRoot = new();
    
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
                options.UseInMemoryDatabase(DatabaseConstant.InMemoryDbName, DbRoot);
            });
            
            var optionsMock = Substitute.For<IOptions<Auth0Options>>();
            optionsMock.Value.Returns(new Auth0Options
            {
                Domain = "test.domain",
                ClientId = "test-id",
                ClientSecret = "test-secret",
                Audience = "test-audience",
                Authority = "https://test.authority",
                ManagementApiUrl = "https://test.management"
            });
            
            var auth0Mock = Substitute.For<IAuth0Service>(); 
            
            auth0Mock.UpdateUserProfile(Arg.Any<string>(), Arg.Any<UpdateUserDto>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);
            
            services.RemoveAll(typeof(IAuth0Service));
            services.AddSingleton(auth0Mock);

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
