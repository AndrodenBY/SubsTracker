using Auth0.AuthenticationApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SubsTracker.API.Auth0;
using SubsTracker.IntegrationTests.Helpers;

namespace SubsTracker.IntegrationTests.Configuration.ServiceConfigs;

public static class Auth0TestConfig
{
    public static void AddFakeAuth0Config(this IConfigurationBuilder config)
    {
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
    }

    public static IServiceCollection ReplaceAuth0Services(this IServiceCollection services)
    {
        services.RemoveAll<AuthenticationApiClient>();
        services.AddSingleton(new AuthenticationApiClient(new Uri("https://fake-ci.auth0.com/")));

        services.RemoveAll<IAuth0Service>();
        services.AddSingleton<IAuth0Service, FakeAuth0Service>();
        
        return services;
    }
    
    public static IServiceCollection AddTestAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "TestAuthScheme";
            options.DefaultChallengeScheme = "TestAuthScheme";
        }).AddScheme<AuthenticationSchemeOptions, TestsAuthHandler>("TestAuthScheme", _ => { });

        services.RemoveAll<JwtBearerOptions>();
        
        return services;
    }
}
