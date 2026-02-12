namespace SubsTracker.UnitTests.UserService;

public class UserServiceDeleteTests : UserServiceTestsBase
{
    [Fact]
    public async Task Delete_WhenUserExists_DeletesUser()
    {
        //Arrange
        var auth0Id = "auth0|test-id";
        var existingUser = Fixture.Build<User>()
            .With(x => x.Auth0Id, auth0Id)
            .Create();

        UserRepository.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>())
            .Returns(existingUser);

        UserRepository.Delete(existingUser, Arg.Any<CancellationToken>())
            .Returns(true);

        //Act
        var result = await Service.Delete(auth0Id, default);

        //Assert
        result.ShouldBeTrue();
        await UserRepository.Received(1).GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>());
        await UserRepository.Received(1).Delete(existingUser, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        //Arrange
        var auth0Id = "auth0|non-existent";

        UserRepository.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(null));

        //Act
        var act = async () => await Service.Delete(auth0Id, default);

        //Assert
        var exception = await act.ShouldThrowAsync<UnknownIdentifierException>();
        exception.Message.ShouldContain(auth0Id);
        await UserRepository.DidNotReceive().Delete(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }
}
