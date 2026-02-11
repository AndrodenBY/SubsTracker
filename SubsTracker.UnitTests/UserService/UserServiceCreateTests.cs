namespace SubsTracker.UnitTests.UserService;

public class UserServiceCreateTests : UserServiceTestsBase
{
    [Fact]
    public async Task Create_WhenUserDoesNotExist_ShouldCreateAndReturnNewUser()
    {
        //Arrange
        var auth0Id = "auth0|123";
        var createDto = Fixture.Create<CreateUserDto>();
        var userEntity = Fixture.Build<User>().With(x => x.Email, createDto.Email).Create();
        var userDto = Fixture.Create<UserDto>();

        UserRepository.GetByPredicate(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(null));

        Mapper.Map<User>(createDto).Returns(userEntity);
        UserRepository.Create(userEntity, Arg.Any<CancellationToken>()).Returns(userEntity);
        Mapper.Map<UserDto>(userEntity).Returns(userDto);

        //Act
        var result = await Service.Create(auth0Id, createDto, default);

        //Assert
        result.ShouldNotBeNull();
        userEntity.Auth0Id.ShouldBe(auth0Id);
        await UserRepository.Received(1).Create(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await UserRepository.DidNotReceive().Update(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_WhenCreateDtoIsNull_ReturnsNull()
    {
        //Act
        var result = await Service.Create(null!, default);

        //Assert
        result.ShouldBeNull();
    }
    
    [Fact]
    public async Task Create_WhenUserExistsWithAuth0Id_ShouldJustReturnExisting()
    {
        //Arrange
        var auth0Id = "auth0|new";
        var createDto = Fixture.Create<CreateUserDto>();
        var existingUser = Fixture.Build<User>()
            .With(x => x.Email, createDto.Email)
            .With(x => x.Auth0Id, "already-has-id")
            .Create();
        var userDto = Fixture.Create<UserDto>();

        UserRepository.GetByPredicate(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(existingUser);

        Mapper.Map<UserDto>(existingUser).Returns(userDto);

        //Act
        var result = await Service.Create(auth0Id, createDto, default);

        //Assert
        await UserRepository.DidNotReceive().Update(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await UserRepository.DidNotReceive().Create(Arg.Any<User>(), Arg.Any<CancellationToken>());
        result.ShouldNotBeNull();
    }
}
