using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using SubsTracker.API.Auth0;

namespace SubsTracker.IntegrationTests.Auth0;

public class Auth0ServiceTests
{
    [Fact]
    public async Task UpdateUserProfile_ShouldExecuteServiceCode()
    {
        // Arrange
        var connectionMock = Substitute.For<IAuthenticationConnection>();
    
        connectionMock.SendAsync<AccessTokenResponse>(
                Arg.Any<HttpMethod>(),
                Arg.Any<Uri>(),
                Arg.Any<object>(),
                Arg.Any<IDictionary<string, string>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AccessTokenResponse { AccessToken = "fake-token" }));

        var auth0Options = new Auth0Options
        {
            Domain = "localhost",
            Authority = "http://127.0.0.1",
            ClientId = "test-id",
            ClientSecret = "test-secret",
            Audience = "test-audience",
            ManagementApiUrl = "http://127.0.0.1"
        };
    
        var options = Options.Create(auth0Options);
        var authClient = new AuthenticationApiClient(new Uri(auth0Options.Authority), connectionMock);
        var service = new Auth0Service(authClient, options);
        var dto = new UpdateUserDto { FirstName = "Ivan"};

        //Act&Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () => 
            await service.UpdateUserProfile("auth0|123", dto, CancellationToken.None)
        );
        
        await connectionMock.Received().SendAsync<AccessTokenResponse>(
            Arg.Any<HttpMethod>(),
            Arg.Any<Uri>(),
            Arg.Any<object>(),
            Arg.Any<IDictionary<string, string>>(),
            Arg.Any<CancellationToken>());
    }
}
