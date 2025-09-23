namespace SubsTracker.UnitTests.UserService;

public class UserServiceGetByIdTests : UserServiceTestsBase
{
    [Fact]
    public async Task GetById_WhenUserExists_ReturnsUser()
    {
        //Arrange
        var existingUser = _fixture.Create<User>();
        var expectedDto = new UserDto { Id = existingUser.Id, FirstName = existingUser.FirstName, Email = existingUser.Email };

        _repository.GetById(existingUser.Id, default).Returns(existingUser);
        _mapper.Map<UserDto>(existingUser).Returns(expectedDto);

        //Act
        var result = await _service.GetById(existingUser.Id, default);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(existingUser.Id);
        result.FirstName.ShouldBe(existingUser.FirstName);
        await _repository.Received(1).GetById(existingUser.Id, default);
    }


    [Fact]
    public async Task GetById_WhenEmptyGuid_ReturnsNull()
    {
        //Arrange
        var emptyId = Guid.Empty;
        
        //Act
        var emptyIdResult = await _service.GetById(emptyId, default);
        
        //Assert
        emptyIdResult.ShouldBeNull();
    }

    [Fact]
    public async Task GetById_WhenUserDoesNotExist_ReturnsNull()
    {
        //Arrange
        var fakeId = Guid.NewGuid();
        
        //Act
        var fakeIdResult = await _service.GetById(fakeId, default);
        
        //Assert
        fakeIdResult.ShouldBeNull();
    }
}
