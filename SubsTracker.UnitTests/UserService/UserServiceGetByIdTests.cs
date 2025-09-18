namespace SubsTracker.UnitTests.UserService;

public class UserServiceGetByIdTests : UserServiceTestsBase
{
    private readonly Guid _userId;
    private readonly User _userEntity;
    private readonly UserDto _userDto;
    
    public UserServiceGetByIdTests()
    {
        _userId = Guid.NewGuid();
        _userEntity = new User { Id = _userId, FirstName = "John", Email = "john@email.com"};
        _userDto = new UserDto { Id = _userId, FirstName = "John" };
        
        _repository.GetById(_userId, default)
            .Returns(Task.FromResult<User?>(_userEntity));
        
        _mapper.Map<UserDto>(_userEntity)
            .Returns(_userDto);
    }

    [Fact]
    public async Task GetById_ShouldReturnUserDto_WhenUserExists()
    {
        //Act
        var result = await _service.GetById(_userId, default);
        
        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(_userEntity.Id);
        result.FirstName.ShouldBe(_userEntity.FirstName);
    }

    [Fact]
    public async Task GetById_ShouldReturnNull_WhenEmptyGuid()
    {
        //Arrange
        var emptyId = Guid.Empty;
        
        //Act
        var emptyIdResult = await _service.GetById(emptyId, default);
        
        //Assert
        emptyIdResult.ShouldBeNull();
    }

    [Fact]
    public async Task GetById_ShouldReturnNull_WhenUserDoesNotExist()
    {
        //Arrange
        var fakeId = Guid.NewGuid();
        
        //Act
        var fakeIdResult = await _service.GetById(fakeId, default);
        
        //Assert
        fakeIdResult.ShouldBeNull();
    }

    [Fact]
    public async Task GetById_WhenCalled_CallsRepositoryExactlyOnce()
    {
        //Act
        await _service.GetById(_userId, default);
        
        //Assert
        await _repository.Received(1).GetById(_userId, default);
        _mapper.Received(1).Map<UserDto>(_userEntity);
    }
}
