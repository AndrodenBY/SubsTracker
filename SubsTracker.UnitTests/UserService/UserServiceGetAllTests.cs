namespace SubsTracker.UnitTests.UserService;

public class UserServiceGetAllTests : UserServiceTestsBase
{
    [Fact]
    public async Task GetAll_WhenFilteredByEmail_ReturnsCorrectUser()
    {
        //Arrange
        var userToFind = Fixture.Create<User>();
        var userDto = Fixture.Build<UserDto>()
            .With(user => user.Email, userToFind.Email)
            .With(user => user.Id, userToFind.Id)
            .With(user => user.FirstName, userToFind.FirstName)
            .Create();

        var filter = new UserFilterDto { Email = userToFind.Email };

        Repository.GetAll(Arg.Any<Expression<Func<User, bool>>>(), default)
           .Returns(new List<User> { userToFind });

        Mapper.Map<List<UserDto>>(Arg.Any<List<User>>())
           .Returns(new List<UserDto> { userDto });

        //Act
        var result = await Service.GetAll(filter, default);

        //Assert
        await Repository.Received(1).GetAll(Arg.Any<Expression<Func<User, bool>>>(), default);
        result.ShouldNotBeNull();
        result.Single().Email.ShouldBe(userToFind.Email);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentEmail_ReturnsEmptyList()
    {
        //Arrange
        var filter = new UserFilterDto { Email = "nonexistent@example.com" };

        Repository.GetAll(Arg.Any<Expression<Func<User, bool>>>(), default)
           .Returns(new List<User>());
        Mapper.Map<List<UserDto>>(Arg.Any<List<User>>()).Returns(new List<UserDto>());

        //Act
        var result = await Service.GetAll(filter, default);

        //Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WhenNoUsers_ReturnsEmptyList()
    {
        //Arrange
        var filter = new UserFilterDto();

        Repository.GetAll(Arg.Any<Expression<Func<User, bool>>>(), default)
           .Returns(new List<User>());
        Mapper.Map<List<UserDto>>(Arg.Any<List<User>>()).Returns(new List<UserDto>());

        //Act
        var result = await Service.GetAll(filter, default);

        //Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WhenFilterIsEmpty_ReturnsAllUsers()
    {
        //Arrange
        var users = Fixture.CreateMany<User>(3).ToList();
        var userDtos = Fixture.CreateMany<UserDto>(3).ToList();

        var filter = new UserFilterDto();

        Repository.GetAll(Arg.Any<Expression<Func<User, bool>>>(), default)
           .Returns(users);
        Mapper.Map<List<UserDto>>(users).Returns(userDtos);

        //Act
        var result = await Service.GetAll(filter, default);

        //Assert
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(3);
        result.ShouldBe(userDtos);
    }
}

