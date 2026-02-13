namespace SubsTracker.UnitTests.UserGroupService;

public class UserGroupServiceTests : UserGroupServiceTestsBase
{
    [Fact]
    public async Task GetAll_WhenFilteredByName_ReturnsCorrectUserGroup()
    {
        //Arrange
        var userGroupToFind = Fixture.Create<UserGroup>();
        var userGroupDto = Fixture.Build<UserGroupDto>()
            .With(userGroup => userGroup.Name, userGroupToFind.Name)
            .With(userGroup => userGroup.Id, userGroupToFind.Id)
            .Create();

        var filter = new UserGroupFilterDto { Name = userGroupToFind.Name };

        GroupRepository.GetAll(Arg.Any<Expression<Func<UserGroup, bool>>>(), default)
            .Returns(new List<UserGroup> { userGroupToFind });
        Mapper.Map<List<UserGroupDto>>(Arg.Any<List<UserGroup>>()).Returns(new List<UserGroupDto> { userGroupDto });

        //Act
        var result = await Service.GetAll(filter, default);

        //Assert
        await GroupRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<UserGroup, bool>>>(),
            default
        );

        result.ShouldNotBeNull();
        result.ShouldHaveSingleItem();
        result.Single().Name.ShouldBe(userGroupToFind.Name);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentName_ReturnsEmptyList()
    {
        //Arrange
        var userGroupToFind = Fixture.Create<UserGroup>();
        Fixture.Build<UserGroupDto>()
            .With(userGroup => userGroup.Name, userGroupToFind.Name)
            .With(userGroup => userGroup.Id, userGroupToFind.Id)
            .Create();

        var filter = new UserGroupFilterDto { Name = "Pv$$YbR3aK3rS123" };

        GroupRepository.GetAll(Arg.Any<Expression<Func<UserGroup, bool>>>(), default)
            .Returns(new List<UserGroup>());
        Mapper.Map<List<UserGroupDto>>(Arg.Any<List<UserGroup>>()).Returns(new List<UserGroupDto>());

        //Act
        var result = await Service.GetAll(filter, default);

        //Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WhenNoUserGroups_ReturnsEmptyList()
    {
        //Arrange
        var filter = new UserGroupFilterDto();

        GroupRepository.GetAll(Arg.Any<Expression<Func<UserGroup, bool>>>(), default)
            .Returns(new List<UserGroup>());
        Mapper.Map<List<UserGroupDto>>(Arg.Any<List<UserGroup>>()).Returns(new List<UserGroupDto>());

        //Act
        var result = await Service.GetAll(filter, default);

        //Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WhenFilterIsEmpty_ReturnsAllUserGroups()
    {
        //Arrange
        var userGroups = Fixture.CreateMany<UserGroup>(3).ToList();
        var userGroupDtos = Fixture.CreateMany<UserGroupDto>(3).ToList();

        var filter = new UserGroupFilterDto();

        GroupRepository.GetAll(Arg.Any<Expression<Func<UserGroup, bool>>>(), default)
            .Returns(userGroups);
        Mapper.Map<List<UserGroupDto>>(userGroups).Returns(userGroupDtos);

        //Act
        var result = await Service.GetAll(filter, default);

        //Assert
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(3);
        result.ShouldBe(userGroupDtos);
    }
    
    [Fact]
    public async Task ShareSubscription_WhenValidData_AddSubscriptionToGroup()
    {
        //Arrange
        var userGroup = Fixture.Build<UserGroup>()
            .With(group => group.SharedSubscriptions, new List<Subscription>())
            .Create();
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(), Price = 9.99m, Content = SubscriptionContent.Design, DueDate = DateOnly.MaxValue,
            Type = SubscriptionType.Free
        };
        var expectedDto = Fixture.Build<UserGroupDto>()
            .With(group => group.Id, userGroup.Id)
            .With(group => group.Name, userGroup.Name)
            .Create();

        GroupRepository.GetFullInfoById(userGroup.Id, default)
            .Returns(userGroup);
        SubscriptionRepository.GetById(subscription.Id, default)
            .Returns(subscription);
        GroupRepository.Update(Arg.Any<UserGroup>(), default)
            .Returns(userGroup);
        Mapper.Map<UserGroupDto>(Arg.Any<UserGroup>()).Returns(expectedDto);

        //Act
        var result = await Service.ShareSubscription(userGroup.Id, subscription.Id, default);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(userGroup.Id);
        await GroupRepository.Received(1)
            .Update(Arg.Is<UserGroup>(g => g.SharedSubscriptions.Contains(subscription)), default);
    }

    [Fact]
    public async Task ShareSubscription_WhenGroupDoesNotExist_ThrowsNotFoundException()
    {
        //Arrange
        var nonExistentGroupId = Guid.NewGuid();

        GroupRepository.GetFullInfoById(nonExistentGroupId, default)
            .Returns(Task.FromResult<UserGroup?>(null));

        //Act
        var result = async () => await Service.ShareSubscription(nonExistentGroupId, Guid.NewGuid(), default);

        //Assert
        await result.ShouldThrowAsync<UnknownIdentifierException>();
    }

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
            Arg.Any<Func<Task<UserGroupDto?>>>(),
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
            Arg.Any<Func<Task<UserGroupDto?>>>(),
            default
        );
    }

    [Fact]
    public async Task GetById_WhenEmptyGuid_ThrowsNotFoundException()
    {
        //Arrange
        var emptyId = Guid.Empty;

        //Act
        var emptyIdResult = async () => await Service.GetById(emptyId, default);

        //Assert
        await Should.ThrowAsync<UnknownIdentifierException>(emptyIdResult);
    }

    [Fact]
    public async Task GetById_WhenUserGroupDoesNotExist_ThrowsNotFoundException()
    {
        //Arrange
        var fakeId = Guid.NewGuid();

        //Act
        var fakeIdResult = async () => await Service.GetById(fakeId, default);

        //Assert
        await Should.ThrowAsync<UnknownIdentifierException>(fakeIdResult);
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
            Arg.Any<Func<Task<UserGroupDto?>>>(),
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
            Arg.Any<Func<Task<UserGroupDto?>>>(),
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
            Arg.Any<Func<Task<UserGroupDto?>>>(),
            default
        );
    }
    
    [Fact]
    public async Task Delete_WhenCorrectModel_DeletesUserGroup()
    {
        //Arrange
        var userGroupEntity = Fixture.Create<UserGroup>();

        GroupRepository.GetById(userGroupEntity.Id, default).Returns(userGroupEntity);
        GroupRepository.Delete(userGroupEntity, default).Returns(true);

        //Act
        var result = await Service.Delete(userGroupEntity.Id, default);

        //Assert
        result.ShouldBeTrue();
        await GroupRepository.Received(1).Delete(userGroupEntity, default);
    }

    [Fact]
    public async Task Delete_WhenEmptyGuid_ThrowsNotFoundException()
    {
        //Arrange
        var emptyId = Guid.Empty;

        GroupRepository.GetById(emptyId, default).Returns((UserGroup?)null);

        //Act
        var result = async () => await Service.Delete(emptyId, default);

        //Assert
        await result.ShouldThrowAsync<UnknownIdentifierException>();
    }
    
    [Fact]
    public async Task Create_WhenCalled_ReturnsCreatedUserGroupDto()
    {
        //Arrange
        var createDto = Fixture.Create<CreateUserGroupDto>();
        var userGroupEntity = Fixture.Build<UserGroup>()
            .With(userGroup => userGroup.Name, createDto.Name)
            .With(userGroup => userGroup.UserId, createDto.UserId)
            .Create();
        var userGroupDto = Fixture.Build<UserGroupDto>()
            .With(userGroup => userGroup.Name, userGroupEntity.Name)
            .With(userGroup => userGroup.Id, userGroupEntity.Id)
            .Create();

        UserRepository.GetById(createDto.UserId, default).Returns(new User { Id = createDto.UserId });
        GroupRepository.Create(Arg.Any<UserGroup>(), default).Returns(userGroupEntity);
        Mapper.Map<UserGroup>(Arg.Any<CreateUserGroupDto>()).Returns(userGroupEntity);
        Mapper.Map<UserGroupDto>(Arg.Any<UserGroup>()).Returns(userGroupDto);

        //Act
        var result = await Service.Create(createDto, default);

        //Assert
        result.ShouldNotBeNull();
        await GroupRepository.Received(1).Create(Arg.Any<UserGroup>(), default);
        result.ShouldBeEquivalentTo(userGroupDto);
    }

    [Fact]
    public async Task Create_WhenEmptyDto_ThrowsValidationException()
    {
        //Arrange
        var createDto = new CreateUserGroupDto { Name = string.Empty, UserId = Guid.Empty };

        //Act & Assert
        await Should.ThrowAsync<InvalidRequestDataException>(async () =>
        {
            await Service.Create(Guid.NewGuid(), createDto, default);
        });
    }

    [Fact]
    public async Task Create_WhenCalled_CallsRepositoryExactlyOnce()
    {
        //Arrange
        var createDto = Fixture.Create<CreateUserGroupDto>();
        var userGroupEntity = Fixture.Build<UserGroup>()
            .With(userGroup => userGroup.Name, createDto.Name)
            .With(userGroup => userGroup.UserId, createDto.UserId)
            .Create();
        var userGroupDto = Fixture.Build<UserGroupDto>()
            .With(userGroup => userGroup.Name, userGroupEntity.Name)
            .With(userGroup => userGroup.Id, userGroupEntity.Id)
            .Create();

        UserRepository.GetById(createDto.UserId, default)
            .Returns(new User { Id = createDto.UserId });
        GroupRepository.Create(Arg.Any<UserGroup>(), default)
            .Returns(userGroupEntity);
        Mapper.Map<UserGroup>(Arg.Any<CreateUserGroupDto>())
            .Returns(userGroupEntity);
        Mapper.Map<UserGroupDto>(Arg.Any<UserGroup>())
            .Returns(userGroupDto);

        //Act
        await Service.Create(createDto, default);

        //Assert
        await GroupRepository.Received(1).Create(Arg.Any<UserGroup>(), default);
    }
    
    [Fact]
    public async Task UnshareSubscription_WhenDataIsValid_RemovesSubscription()
    {
        //Arrange
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(), Type = SubscriptionType.Free, Content = SubscriptionContent.Design,
            DueDate = DateOnly.MinValue, Price = 9.99m
        };
        var userGroup = Fixture.Build<UserGroup>()
            .With(group => group.SharedSubscriptions, new List<Subscription> { subscription })
            .Create();
        var expectedDto = Fixture.Build<UserGroupDto>()
            .With(group => group.Id, userGroup.Id)
            .With(group => group.Name, userGroup.Name)
            .Create();

        GroupRepository.GetFullInfoById(userGroup.Id, default)
            .Returns(userGroup);
        GroupRepository.Update(Arg.Any<UserGroup>(), default)
            .Returns(userGroup);

        Mapper.Map<UserGroupDto>(Arg.Any<UserGroup>()).Returns(expectedDto);

        //Act
        var result = await Service.UnshareSubscription(userGroup.Id, subscription.Id, default);

        //Assert
        result.ShouldNotBeNull();
        await GroupRepository.Received(1)
            .Update(Arg.Is<UserGroup>(g => !g.SharedSubscriptions.Contains(subscription)), default);
    }

    [Fact]
    public async Task UnshareSubscription_WhenGroupDoesNotExist_ThrowsNotFoundException()
    {
        //Arrange
        GroupRepository.GetById(Arg.Any<Guid>(), default)
            .Returns((UserGroup?)null);

        //Act
        var result = async () => await Service.UnshareSubscription(Guid.NewGuid(), Guid.NewGuid(), default);

        //Assert
        await result.ShouldThrowAsync<UnknownIdentifierException>();
    }
    
    [Fact]
    public async Task Update_WhenValidModel_ReturnsUpdatedUserGroupDto()
    {
        //Arrange
        var userGroupEntity = Fixture.Create<UserGroup>();
        var updateDto = Fixture.Build<UpdateUserGroupDto>()
            .With(userGroup => userGroup.Id, userGroupEntity.Id)
            .Create();
        var userGroupDto = Fixture.Build<UserGroupDto>()
            .With(userGroup => userGroup.Name, updateDto.Name)
            .With(userGroup => userGroup.Id, updateDto.Id)
            .Create();

        GroupRepository.GetById(updateDto.Id, default).Returns(userGroupEntity);
        GroupRepository.Update(Arg.Any<UserGroup>(), default).Returns(userGroupEntity);
        Mapper.Map(updateDto, userGroupEntity).Returns(userGroupEntity);
        Mapper.Map<UserGroupDto>(userGroupEntity).Returns(userGroupDto);

        //Act
        var result = await Service.Update(updateDto.Id, updateDto, default);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBeEquivalentTo(userGroupEntity.Id);
        result.Name.ShouldBe(updateDto.Name);
        result.Name.ShouldNotBe(userGroupEntity.Name);
        await GroupRepository.Received(1).Update(Arg.Any<UserGroup>(), default);
    }

    [Fact]
    public async Task Update_WhenGivenEmptyModel_ReturnsNotFoundException()
    {
        //Arrange
        var emptyDto = new UpdateUserGroupDto();

        //Act
        var result = async () => await Service.Update(Guid.Empty, emptyDto, default);

        //Assert
        await result.ShouldThrowAsync<UnknownIdentifierException>();
    }
}
