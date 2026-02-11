using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Microsoft.Extensions.Options;
using NSubstitute;
using SubsTracker.API.Auth0;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.Domain.Options;
using Xunit;

namespace SubsTracker.IntegrationTests.Auth0;

public class Auth0ServiceTests
{
    [Fact]
    public async Task UpdateUserProfile_ShouldExecuteServiceCode_ForCoverage()
    {
        //Arrange
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
            Domain = "fake-ci.auth0.com",
            Authority = "https://fake-ci.auth0.com/",
            ClientId = "test-id",
            ClientSecret = "test-secret",
            Audience = "test-audience",
            ManagementApiUrl = "https://fake-ci.auth0.com/api/v2/"
        };
    
        var options = Options.Create(auth0Options);
        var authClient = new AuthenticationApiClient(new Uri(auth0Options.Authority), connectionMock);
        var service = new Auth0Service(authClient, options);
        var dto = new UpdateUserDto { FirstName = "Ivan", Email = "ivan@test.com" };

        //Act
        await service.UpdateUserProfile("auth0|123", dto, CancellationToken.None);
            
        //Assert
        await connectionMock.Received().SendAsync<AccessTokenResponse>(
            Arg.Any<HttpMethod>(),
            Arg.Any<Uri>(),
            Arg.Any<object>(),
            Arg.Any<IDictionary<string, string>>(),
            Arg.Any<CancellationToken>());
    }
}
