namespace SubsTracker.UnitTests.UserService;

public class UserServiceGetByIdTests : UserServiceTestsBase
{
    [Fact]
    public async Task GetById_WhenUserExists_ReturnsUser()
    {
        //Arrange
        var existingUser = Fixture.Create<User>();
        var expectedDto = Fixture.Build<UserDto>()
            .With(user => user.Id, existingUser.Id)
            .With(user => user.FirstName, existingUser.FirstName)
            .With(user => user.Email, existingUser.Email)
            .Create();
        
        var cacheKey = $"{existingUser.Id}_{nameof(User)}";
        
        CacheService.GetData<UserDto>(cacheKey, default)
            .Returns((UserDto)null!);

        Repository.GetById(existingUser.Id, default).Returns(existingUser);
        Mapper.Map<UserDto>(existingUser).Returns(expectedDto);

        //Act
        var result = await Service.GetById(existingUser.Id, default);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(existingUser.Id);
        result.FirstName.ShouldBe(existingUser.FirstName);
        
        await Repository.Received(1).GetById(existingUser.Id, default);
        await CacheService.Received(1).GetData<UserDto>(cacheKey, default);
        await CacheService.Received(1).SetData(
            Arg.Is<string>(key => key == cacheKey), 
            Arg.Is<UserDto>(dto => dto.Id == existingUser.Id && dto.FirstName == existingUser.FirstName), 
            Arg.Is<TimeSpan>(ts => ts == TimeSpan.FromMinutes(3)),
            default
        );
    }

    [Fact]
    public async Task GetById_WhenEmptyGuid_ReturnsNull()
    {
        //Arrange
        var emptyId = Guid.Empty;

        //Act
        var emptyIdResult = await Service.GetById(emptyId, default);

        //Assert
        emptyIdResult.ShouldBeNull();
    }

    [Fact]
    public async Task GetById_WhenUserDoesNotExist_ReturnsNull()
    {
        //Arrange
        var fakeId = Guid.NewGuid();

        //Act
        var fakeIdResult = await Service.GetById(fakeId, default);

        //Assert
        fakeIdResult.ShouldBeNull();
    }
}

