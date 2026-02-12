

namespace SubsTracker.UnitTests.UserService;

public class UserServiceGetByAuth0IdTests : UserServiceTestsBase
{
    [Fact]
    public async Task GetByAuth0Id_WhenUserExists_ReturnsMappedUserDto()
    {
        //Arrange
        var auth0Id = "auth0|661f123456789";
        var existingUser = Fixture.Create<User>();
        var expectedDto = Fixture.Create<UserDto>();

        UserRepository.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>())
            .Returns(existingUser);
            
        Mapper.Map<UserDto>(existingUser)
            .Returns(expectedDto);

        //Act
        var result = await Service.GetByAuth0Id(auth0Id, default);

        //Assert
        result.ShouldNotBeNull();
        result.ShouldBeEquivalentTo(expectedDto);
        await UserRepository.Received(1).GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>());
        Mapper.Received(1).Map<UserDto>(existingUser);
    }

    [Fact]
    public async Task GetByAuth0Id_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        //Arrange
        var nonExistentAuth0Id = "non-existent-id";

        UserRepository.GetByAuth0Id(nonExistentAuth0Id, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        //Act
        var act = () => Service.GetByAuth0Id(nonExistentAuth0Id, default);

        //Assert
        var exception = await Should.ThrowAsync<UnknowIdentifierException>(act);
        exception.Message.ShouldContain(nonExistentAuth0Id);
        Mapper.DidNotReceive().Map<UserDto>(Arg.Any<User>());
    }

    [Fact]
    public async Task GetByAuth0Id_WhenAuth0IdIsEmpty_ThrowsNotFoundException()
    {
        //Arrange
        var emptyAuth0Id = string.Empty;

        UserRepository.GetByAuth0Id(emptyAuth0Id, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        //Act
        var act = () => Service.GetByAuth0Id(emptyAuth0Id, default);

        //Assert
        await Should.ThrowAsync<UnknowIdentifierException>(act);
    }
    
    [Fact]
    public async Task GetByAuth0Id_WhenCancellationTokenIsCancelled_ThrowsTaskCanceledException()
    {
        //Arrange
        var auth0Id = "auth0|cancel-test";
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel(); 
        
        UserRepository.GetByAuth0Id(auth0Id, cancellationTokenSource.Token)
            .Returns(Task.FromCanceled<User?>(cancellationTokenSource.Token));

        //Act
        var act = () => Service.GetByAuth0Id(auth0Id, cancellationTokenSource.Token);

        //Assert
        await Should.ThrowAsync<OperationCanceledException>(act);
    }
}
