namespace SubsTracker.UnitTests.UserGroupService;

public class UserGroupServiceGetByIdTests : UserGroupServiceTestsBase
{
    [Fact]
    public async Task GetById_WhenUserGroupExists_ReturnsUserGroupDto()
    {
        //Arrange
        var userGroupDto = Fixture.Create<UserGroupDto>();
        var userGroup = Fixture.Build<UserGroup>()
            .With(x => x.Id, userGroupDto.Id)
            .With(x => x.Name, userGroupDto.Name)
            .Create();

        var cacheKey = $"{userGroupDto.Id}:{nameof(UserGroup)}";

        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<UserGroupDto>>>(),
            default
        )!.Returns(callInfo =>
        {
            var factory = callInfo.Arg<Func<Task<UserGroupDto>>>();
            return factory();
        });
        GroupRepository.GetById(userGroupDto.Id, default)
            .Returns(userGroup);

        Mapper.Map<UserGroupDto>(userGroup)
            .Returns(userGroupDto);

        //Act
        var result = await Service.GetById(userGroupDto.Id, default);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(userGroupDto.Id);
        result.Name.ShouldBe(userGroupDto.Name);
        await CacheService.Received(1).CacheDataWithLock(
            cacheKey,
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<UserGroupDto>>>(),
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
    public async Task GetById_WhenUserGroupDoesNotExist_ReturnsNull()
    {
        //Arrange
        var fakeId = Guid.NewGuid();

        //Act
        var fakeIdResult = await Service.GetById(fakeId, default);

        //Assert
        fakeIdResult.ShouldBeNull();
    }

    [Fact]
    public async Task GetById_WhenCalled_CallsRepositoryExactlyOnce()
    {
        //Arrange
        var userGroupDto = Fixture.Create<UserGroupDto>();
        var userGroup = Fixture.Build<UserGroup>()
            .With(x => x.Id, userGroupDto.Id)
            .With(x => x.Name, userGroupDto.Name)
            .Create();

        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<UserGroupDto>>>(),
            default
        )!.Returns(callInfo =>
        {
            var factory = callInfo.Arg<Func<Task<UserGroupDto>>>();
            return factory();
        });
        GroupRepository.GetById(userGroupDto.Id, default)
            .Returns(userGroup);

        Mapper.Map<UserGroupDto>(userGroup)
            .Returns(userGroupDto);

        //Act
        await Service.GetById(userGroupDto.Id, default);

        //Assert
        await GroupRepository.Received(1).GetById(userGroup.Id, default);
        Mapper.Received(1).Map<UserGroupDto>(userGroup);
    }

    [Fact]
    public async Task GetById_WhenCacheHit_ReturnsCachedDataAndSkipsRepo()
    {
        //Arrange
        var cachedDto = Fixture.Create<UserGroupDto>();
        var cacheKey = RedisKeySetter.SetCacheKey<UserGroupDto>(cachedDto.Id);

        CacheService.CacheDataWithLock(
            cacheKey,
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<UserGroupDto>>>(),
            default
        ).Returns(cachedDto);

        //Act
        var result = await Service.GetFullInfoById(cachedDto.Id, default);

        //Assert
        result.ShouldBe(cachedDto);

        await GroupRepository.DidNotReceive().GetFullInfoById(Arg.Any<Guid>(), default);
        await CacheService.Received(1).CacheDataWithLock(
            cacheKey,
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<UserGroupDto>>>(),
            default
        );
    }
}
