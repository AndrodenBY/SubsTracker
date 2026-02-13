using System.Net.Http.Headers;
using SubsTracker.IntegrationTests.Helpers;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace SubsTracker.IntegrationTests.Configuration.WebApplicationFactory;

public class TestsWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:15-alpine")
        .WithDatabase("subs_test_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();
    
    private readonly RedisContainer _redisContainer = new RedisBuilder("redis:7-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_dbContainer.StartAsync(), _redisContainer.StartAsync());
    }

    public new async Task DisposeAsync()
    {
        await Task.WhenAll(
            _dbContainer.DisposeAsync().AsTask(), 
            _redisContainer.DisposeAsync().AsTask()
        );
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Redis"] = _redisContainer.GetConnectionString()
            });
            
            config.AddFakeAuth0Config();
        });

        builder.ConfigureServices(services =>
        {
            services.ReplaceAuth0Services()
                .AddTestAuthentication()
                .ReplaceMassTransit()
                .ReplaceDatabase(_dbContainer.GetConnectionString())
                .ConfigureTestContainerCache(_redisContainer.GetConnectionString());
        });
    }

    public HttpClient CreateAuthenticatedClient(string auth0Id = TestsAuthHandler.DefaultAuth0Id)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestAuthScheme");
        
        client.DefaultRequestHeaders.Remove(TestsAuthHandler.Auth0Header);
        client.DefaultRequestHeaders.TryAddWithoutValidation(TestsAuthHandler.Auth0Header, auth0Id);
        
        return client;
    }
}
