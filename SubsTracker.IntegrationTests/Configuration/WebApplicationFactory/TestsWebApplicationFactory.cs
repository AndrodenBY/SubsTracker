using System.Net.Http.Headers;
using SubsTracker.IntegrationTests.Helpers;

namespace SubsTracker.IntegrationTests.Configuration.WebApplicationFactory;

public class TestsWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTest");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.Sources.Clear();
            config.AddFakeAuth0Config();
        });

        builder.ConfigureServices(services =>
        {
            services.ReplaceDatabase()
                .ReplaceAuth0Services()
                .AddTestAuthentication()
                .ReplaceMassTransit()
                .ReplaceCache();
        });
    }

    public HttpClient CreateAuthenticatedClient(string auth0Id = TestsAuthHandler.DefaultAuth0Id)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestAuthScheme");
        
        client.DefaultRequestHeaders.Add(TestsAuthHandler.Auth0Header, auth0Id);
        return client;
    }
}
