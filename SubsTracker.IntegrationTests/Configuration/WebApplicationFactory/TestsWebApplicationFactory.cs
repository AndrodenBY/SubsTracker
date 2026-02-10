using System.Net.Http.Headers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
                .ReplaceUserUpdateOrchestrator();
        });
    }
    
    public void Override<TService>(TService instance) where TService : class
    {
        WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<TService>();
                services.AddSingleton(instance);
            });
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
