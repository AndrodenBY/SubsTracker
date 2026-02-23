using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SubsTracker.IntegrationTests.Helpers;

public class TestsAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string DefaultAuth0Id = "auth0|test-user-12345";
    public const string Auth0Header = "X-Test-Auth0Id";
    
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Context.Request.Headers.ContainsKey("X-Skip-Auth"))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, DefaultAuth0Id),
            new Claim(ClaimTypes.Name, "testuser@example.com"),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, "TestAuthScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestAuthScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
