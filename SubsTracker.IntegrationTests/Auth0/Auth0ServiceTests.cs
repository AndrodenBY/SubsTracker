using SubsTracker.API.Auth0;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.IntegrationTests.Configuration;
using Xunit;

namespace SubsTracker.IntegrationTests.Auth0;

public class Auth0ServiceTests
{
    private readonly IAuth0Service _auth0Service;

    public Auth0ServiceTests()
    {
        _auth0Service = new FakeAuth0Service();
    }

    [Fact]
    public async Task GetClientCredentialsToken_ReturnsFakeToken()
    {
        // Act
        var token = await _auth0Service.GetClientCredentialsToken(CancellationToken.None);

        // Assert
        Assert.Equal("fake-ci-token-12345", token);
    }

    [Fact]
    public async Task UpdateUserProfile_CompletesSuccessfully()
    {
        // Arrange
        var dto = new UpdateUserDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com"
        };

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            _auth0Service.UpdateUserProfile("auth0|fake-user-id", dto, CancellationToken.None)
        );

        Assert.Null(exception);
    }
}

