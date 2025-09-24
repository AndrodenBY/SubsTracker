namespace SubsTracker.UnitTests.UserService;

public class UserServiceGetAllTests : UserServiceTestsBase
{
    [Fact]
    public async Task GetAll_WhenFilteredByEmail_ReturnsCorrectUser()
    {
        //Arrange
        var userToFind = _fixture.Create<User>();
        var userDto = _fixture.Build<UserDto>()
            .With(user => user.Email, userToFind.Email)
            .With(user => user.Id, userToFind.Id)
            .With(user => user.FirstName, userToFind.FirstName)
            .Create();
            
        var filter = new UserFilterDto { Email = userToFind.Email };

        _repository.GetAll(Arg.Any<Expression<Func<User, bool>>>(), default)
            .Returns(new List<User> { userToFind });

        _mapper.Map<List<UserDto>>(Arg.Any<List<User>>())
            .Returns(new List<UserDto> { userDto });

        //Act
        var result = await _service.GetAll(filter, default);

        //Assert
        await _repository.Received(1).GetAll(Arg.Any<Expression<Func<User, bool>>>(), default);
        result.ShouldNotBeNull();
        result.Single().Email.ShouldBe(userToFind.Email);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentEmail_ReturnsEmptyList()
    {
        //Arrange
        var filter = new UserFilterDto { Email = "nonexistent@example.com" };

        _repository.GetAll(Arg.Any<Expression<Func<User, bool>>>(), default)
            .Returns(new List<User>());
        _mapper.Map<List<UserDto>>(Arg.Any<List<User>>()).Returns(new List<UserDto>());
        
        //Act
        var result = await _service.GetAll(filter, default);
        
        //Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WhenNoUsers_ReturnsEmptyList()
    {
        //Arrange
        var filter = new UserFilterDto();
        
        _repository.GetAll(Arg.Any<Expression<Func<User, bool>>>(), default)
            .Returns(new List<User>());
        _mapper.Map<List<UserDto>>(Arg.Any<List<User>>()).Returns(new List<UserDto>());
        
        //Act
        var result = await _service.GetAll(filter, default);
        
        //Assert
        result.ShouldBeEmpty();
    }
    
    [Fact]
    public async Task GetAll_WhenFilterIsEmpty_ReturnsAllUsers()
    {
        //Arrange
        var users = _fixture.CreateMany<User>(3).ToList();
        var userDtos = _fixture.CreateMany<UserDto>(3).ToList();
        
        var filter = new UserFilterDto();

        _repository.GetAll(Arg.Any<Expression<Func<User, bool>>>(), default)
            .Returns(users);
        _mapper.Map<List<UserDto>>(users).Returns(userDtos);

        //Act
        var result = await _service.GetAll(filter, default);

        //Assert
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(3);
        result.ShouldBe(userDtos);
    }
}

