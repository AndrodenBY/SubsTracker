using SubsTracker.API.Auth0;
using SubsTracker.API.Helpers;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.Interfaces.User;

namespace SubsTracker.IntegrationTests.Auth0;

public class UserUpdateOrchestratorTests
{
    [Fact]
    public async Task FullUserUpdate_ShouldCall_Auth0Service_And_UserService()
    {
        //Arrange
        var auth0Service = Substitute.For<IAuth0Service>();
        var userService = Substitute.For<IUserService>();

        var orchestrator = new UserUpdateOrchestrator(auth0Service, userService);

        var auth0Id = "auth0|123";
        var dto = new UpdateUserDto();

        userService.Update(auth0Id, dto, Arg.Any<CancellationToken>())
            .Returns(new UserDto
            {
                FirstName = "Test",
                LastName = "User"
            });

        //Act
        await orchestrator.FullUserUpdate(auth0Id, dto, default);

        //Assert
        await auth0Service.Received(1)
            .UpdateUserProfile(auth0Id, dto, Arg.Any<CancellationToken>());

        await userService.Received(1)
            .Update(auth0Id, dto, Arg.Any<CancellationToken>());
    }
}
