using System.Net.Http.Headers;

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

    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("TestAuthScheme");
        return client;
    }
}
