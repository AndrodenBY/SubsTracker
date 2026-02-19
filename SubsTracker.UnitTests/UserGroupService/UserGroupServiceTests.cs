using SubsTracker.BLL.Handlers.Signals.Group;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.UnitTests.UserGroupService;

public class UserGroupServiceTests : UserGroupServiceTestsBase
{
    [Fact]
    public async Task GetAll_WhenFilteredByName_ReturnsCorrectUserGroup()
    {
        //Arrange
        var userGroupToFind = Fixture.Create<UserGroup>();
        var userGroupDto = Fixture.Build<UserGroupDto>()
            .With(ug => ug.Id, userGroupToFind.Id)
            .With(ug => ug.Name, userGroupToFind.Name)
            .Create();

        var filter = new UserGroupFilterDto { Name = userGroupToFind.Name };
        var paginationParams = new PaginationParameters { PageNumber = 1, PageSize = 10 };
        
        var paginatedResult = new PaginatedList<UserGroup>(
        [userGroupToFind], 
            PageNumber: 1, 
            PageSize: 10, 
            PageCount: 1, 
            TotalCount: 1
        );
        
        GroupRepository.GetAll(
                Arg.Any<Expression<Func<UserGroup, bool>>>(),
                Arg.Is<PaginationParameters>(p => p.PageNumber == 1),
                Arg.Any<CancellationToken>()
            )
            .Returns(paginatedResult);
        
        Mapper.Map<UserGroupDto>(userGroupToFind).Returns(userGroupDto);

        //Act
        var result = await Service.GetAll(filter, paginationParams, CancellationToken.None);

        //Assert
        await GroupRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<UserGroup, bool>>>(),
            Arg.Any<PaginationParameters>(),
            Arg.Any<CancellationToken>()
        );

        result.ShouldNotBeNull();
        result.Items.ShouldHaveSingleItem();
        result.Items.First().Name.ShouldBe(userGroupToFind.Name);
        result.TotalCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentName_ReturnsEmptyPaginatedList()
    {
        //Arrange
        var filter = new UserGroupFilterDto { Name = "Pv$$YbR3aK3rS123" };
        var paginationParams = new PaginationParameters { PageNumber = 1, PageSize = 10 };

        var paginatedResult = new PaginatedList<UserGroup>(
            new List<UserGroup>(), 
            PageNumber: 1, 
            PageSize: 10, 
            PageCount: 0, 
            TotalCount: 0
        );

        GroupRepository.GetAll(
                Arg.Any<Expression<Func<UserGroup, bool>>>(),
                Arg.Is<PaginationParameters>(p => p.PageNumber == 1),
                Arg.Any<CancellationToken>()
            )
            .Returns(paginatedResult);

        //Act
        var result = await Service.GetAll(filter, paginationParams, CancellationToken.None);

        //Assert
        await GroupRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<UserGroup, bool>>>(),
            Arg.Any<PaginationParameters>(),
            Arg.Any<CancellationToken>()
        );

        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task GetAll_WhenNoUserGroups_ReturnsEmptyPaginatedList()
    {
        //Arrange
        var filter = new UserGroupFilterDto();
        var paginationParams = new PaginationParameters { PageNumber = 1, PageSize = 10 };

        var paginatedResult = new PaginatedList<UserGroup>(
            new List<UserGroup>(), 
            PageNumber: 1, 
            PageSize: 10, 
            PageCount: 0, 
            TotalCount: 0
        );

        GroupRepository.GetAll(
                Arg.Any<Expression<Func<UserGroup, bool>>>(),
                Arg.Any<PaginationParameters>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(paginatedResult);

        //Act
        var result = await Service.GetAll(filter, paginationParams, CancellationToken.None);

        //Assert
        await GroupRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<UserGroup, bool>>>(),
            Arg.Any<PaginationParameters>(),
            Arg.Any<CancellationToken>()
        );

        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task GetAll_WhenFilterIsEmpty_ReturnsAllPaginatedUserGroups()
    {
        //Arrange
        var userGroups = Fixture.CreateMany<UserGroup>(3).ToList();
        var userGroupDtos = Fixture.CreateMany<UserGroupDto>(3).ToList();

        var filter = new UserGroupFilterDto();
        var paginationParams = new PaginationParameters { PageNumber = 1, PageSize = 10 };

        var paginatedResult = new PaginatedList<UserGroup>(
            userGroups, 
            PageNumber: 1, 
            PageSize: 10, 
            PageCount: 1, 
            TotalCount: 3
        );

        GroupRepository.GetAll(
                Arg.Any<Expression<Func<UserGroup, bool>>>(),
                Arg.Any<PaginationParameters>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(paginatedResult);

        Mapper.Map<UserGroupDto>(userGroups[0]).Returns(userGroupDtos[0]);
        Mapper.Map<UserGroupDto>(userGroups[1]).Returns(userGroupDtos[1]);
        Mapper.Map<UserGroupDto>(userGroups[2]).Returns(userGroupDtos[2]);

        //Act
        var result = await Service.GetAll(filter, paginationParams, CancellationToken.None);

        //Assert
        await GroupRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<UserGroup, bool>>>(),
            Arg.Any<PaginationParameters>(),
            Arg.Any<CancellationToken>()
        );

        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(3);
        result.TotalCount.ShouldBe(3);
        result.Items.ShouldBe(userGroupDtos);
    }
    
    [Fact]
    public async Task ShareSubscription_WhenValidData_AddSubscriptionToGroup()
    {
        //Arrange
        var groupId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ct = CancellationToken.None;

        var userGroup = Fixture.Build<UserGroup>()
            .With(g => g.Id, groupId)
            .With(g => g.UserId, userId)
            .With(g => g.SharedSubscriptions, [])
            .Create();

        var subscription = Fixture.Build<Subscription>()
            .With(s => s.Id, subscriptionId)
            .Create();

        var expectedDto = Fixture.Build<UserGroupDto>()
            .With(dto => dto.Id, groupId)
            .Create();

        GroupRepository.GetFullInfoById(groupId, ct).Returns(userGroup);
        SubscriptionRepository.GetById(subscriptionId, ct).Returns(subscription);
        GroupRepository.Update(userGroup, ct).Returns(userGroup);
        Mapper.Map<UserGroupDto>(userGroup).Returns(expectedDto);

        //Act
        var result = await Service.ShareSubscription(groupId, subscriptionId, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(groupId);
        
        await GroupRepository.Received(1).Update(
            Arg.Is<UserGroup>(g => g.SharedSubscriptions != null && g.SharedSubscriptions.Contains(subscription)), 
            ct);

        await Mediator.Received(1).Publish(
            Arg.Is<GroupUpdatedSignal>(s => s.GroupId == groupId && s.UserId == userId), 
            ct);
    }

    [Fact]
    public async Task ShareSubscription_WhenGroupDoesNotExist_ThrowsUnknownIdentifierException()
    {
        //Arrange
        var groupId = Guid.NewGuid();
        var ct = CancellationToken.None;

        GroupRepository.GetFullInfoById(groupId, ct).Returns((UserGroup?)null);

        //Act & Assert
        await Should.ThrowAsync<UnknownIdentifierException>(async () =>
        {
            await Service.ShareSubscription(groupId, Guid.NewGuid(), ct);
        });

        await GroupRepository.DidNotReceive().Update(Arg.Any<UserGroup>(), ct);
        await Mediator.DidNotReceive().Publish(Arg.Any<GroupUpdatedSignal>(), ct);
    }

    [Fact]
    public async Task ShareSubscription_WhenAlreadyShared_ThrowsPolicyViolationException()
    {
        //Arrange
        var groupId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var ct = CancellationToken.None;

        var subscription = Fixture.Build<Subscription>()
            .With(s => s.Id, subscriptionId)
            .Create();

        var userGroup = Fixture.Build<UserGroup>()
            .With(g => g.Id, groupId)
            .With(g => g.SharedSubscriptions, [subscription])
            .Create();

        GroupRepository.GetFullInfoById(groupId, ct).Returns(userGroup);

        //Act & Assert
        await Should.ThrowAsync<PolicyViolationException>(async () =>
        {
            await Service.ShareSubscription(groupId, subscriptionId, ct);
        });

        await SubscriptionRepository.DidNotReceive().GetById(Arg.Any<Guid>(), ct);
        await GroupRepository.DidNotReceive().Update(Arg.Any<UserGroup>(), ct);
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
            Arg.Any<Func<Task<UserGroupDto?>>>(),
            CancellationToken.None
        )!.Returns(callInfo =>
        {
            var factory = callInfo.Arg<Func<Task<UserGroupDto>>>();
            return factory();
        });
        GroupRepository.GetById(userGroupDto.Id, CancellationToken.None)
            .Returns(userGroup);

        Mapper.Map<UserGroupDto>(userGroup)
            .Returns(userGroupDto);

        //Act
        var result = await Service.GetById(userGroupDto.Id, CancellationToken.None);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(userGroupDto.Id);
        result.Name.ShouldBe(userGroupDto.Name);
        await CacheService.Received(1).CacheDataWithLock(
            cacheKey,
            Arg.Any<Func<Task<UserGroupDto?>>>(),
            CancellationToken.None
        );
    }

    [Fact]
    public async Task GetById_WhenEmptyGuid_ThrowsNotFoundException()
    {
        //Arrange
        var emptyId = Guid.Empty;

        //Act
        var emptyIdResult = async () => await Service.GetById(emptyId, CancellationToken.None);

        //Assert
        await Should.ThrowAsync<UnknownIdentifierException>(emptyIdResult);
    }

    [Fact]
    public async Task GetById_WhenUserGroupDoesNotExist_ThrowsNotFoundException()
    {
        //Arrange
        var fakeId = Guid.NewGuid();

        //Act
        var fakeIdResult = async () => await Service.GetById(fakeId, CancellationToken.None);

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
            Arg.Any<Func<Task<UserGroupDto?>>>(),
            CancellationToken.None
        )!.Returns(callInfo =>
        {
            var factory = callInfo.Arg<Func<Task<UserGroupDto>>>();
            return factory();
        });
        GroupRepository.GetById(userGroupDto.Id, CancellationToken.None)
            .Returns(userGroup);

        Mapper.Map<UserGroupDto>(userGroup)
            .Returns(userGroupDto);

        //Act
        await Service.GetById(userGroupDto.Id, CancellationToken.None);

        //Assert
        await GroupRepository.Received(1).GetById(userGroup.Id, CancellationToken.None);
        Mapper.Received(1).Map<UserGroupDto>(userGroup);
    }

    [Fact]
    public async Task GetById_WhenCacheHit_ReturnsCachedDataAndSkipsRepo()
    {
        //Arrange
        var cachedDto = Fixture.Create<UserGroupDto>();
        var cacheKey = RedisKeySetter.SetCacheKey<UserGroup>(cachedDto.Id);

        CacheService.CacheDataWithLock(
            cacheKey,
            Arg.Any<Func<Task<UserGroupDto?>>>(),
            CancellationToken.None
        ).Returns(cachedDto);

        //Act
        var result = await Service.GetFullInfoById(cachedDto.Id, CancellationToken.None);

        //Assert
        result.ShouldBe(cachedDto);

        await GroupRepository.DidNotReceive().GetFullInfoById(Arg.Any<Guid>(), CancellationToken.None);
        await CacheService.Received(1).CacheDataWithLock(
            cacheKey,
            Arg.Any<Func<Task<UserGroupDto?>>>(),
            CancellationToken.None
        );
    }
    
    [Fact]
    public async Task Delete_WhenCorrectModel_DeletesUserGroup()
    {
        //Arrange
        var userGroupEntity = Fixture.Create<UserGroup>();

        GroupRepository.GetById(userGroupEntity.Id, CancellationToken.None).Returns(userGroupEntity);
        GroupRepository.Delete(userGroupEntity, CancellationToken.None).Returns(true);

        //Act
        var result = await Service.Delete(userGroupEntity.Id, CancellationToken.None);

        //Assert
        result.ShouldBeTrue();
        await GroupRepository.Received(1).Delete(userGroupEntity, CancellationToken.None);
    }

    [Fact]
    public async Task Delete_WhenEmptyGuid_ThrowsNotFoundException()
    {
        //Arrange
        var emptyId = Guid.Empty;

        GroupRepository.GetById(emptyId, CancellationToken.None).Returns((UserGroup?)null);

        //Act
        var result = async () => await Service.Delete(emptyId, CancellationToken.None);

        //Assert
        await result.ShouldThrowAsync<UnknownIdentifierException>();
    }
    
    [Fact]
    public async Task Create_WhenCalled_ReturnsCreatedUserGroupDto()
    {
        //Arrange
        var auth0Id = Fixture.Create<string>();
        var createDto = Fixture.Create<CreateUserGroupDto>();
        var ct = CancellationToken.None;

        var existingUser = Fixture.Build<User>()
            .With(u => u.Id, createDto.UserId)
            .Create();

        var userGroupEntity = Fixture.Build<UserGroup>()
            .With(g => g.Name, createDto.Name)
            .With(g => g.UserId, existingUser.Id)
            .Create();

        var userGroupDto = Fixture.Build<UserGroupDto>()
            .With(dto => dto.Name, userGroupEntity.Name)
            .With(dto => dto.Id, userGroupEntity.Id)
            .Create();

        UserRepository.GetByAuth0Id(auth0Id, ct).Returns(existingUser);
        
        Mapper.Map<UserGroup>(createDto).Returns(userGroupEntity);
        GroupRepository.Create(userGroupEntity, ct).Returns(userGroupEntity);
        Mapper.Map<UserGroupDto>(userGroupEntity).Returns(userGroupDto);

        //Act
        var result = await Service.Create(auth0Id, createDto, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(userGroupDto.Id);
        
        await GroupRepository.Received(1).Create(userGroupEntity, ct);
        await Mediator.Received(1).Publish(Arg.Is<GroupCreatedSignal>(s => 
            s.GroupId == userGroupEntity.Id && 
            s.UserId == existingUser.Id), ct);
    }

    [Fact]
    public async Task Create_WhenUserDoesNotExist_ThrowsInvalidRequestDataException()
    {
        //Arrange
        var auth0Id = Fixture.Create<string>();
        var createDto = Fixture.Create<CreateUserGroupDto>();
        var ct = CancellationToken.None;

        UserRepository.GetByAuth0Id(auth0Id, ct).Returns((User?)null);

        //Act & Assert
        await Should.ThrowAsync<InvalidRequestDataException>(async () =>
        {
            await Service.Create(auth0Id, createDto, ct);
        });

        await GroupRepository.DidNotReceive().Create(Arg.Any<UserGroup>(), ct);
        await Mediator.DidNotReceive().Publish(Arg.Any<GroupCreatedSignal>(), ct);
    }

    [Fact]
    public async Task Create_WhenCalled_CallsDependenciesExactlyOnce()
    {
        //Arrange
        var auth0Id = Fixture.Create<string>();
        var createDto = Fixture.Create<CreateUserGroupDto>();
        var ct = CancellationToken.None;
        var existingUser = Fixture.Create<User>();

        var userGroupEntity = Fixture.Build<UserGroup>()
            .With(g => g.Id, Guid.NewGuid())
            .With(g => g.UserId, existingUser.Id)
            .Create();

        UserRepository.GetByAuth0Id(auth0Id, ct).Returns(existingUser);
        Mapper.Map<UserGroup>(createDto).Returns(userGroupEntity);
        GroupRepository.Create(userGroupEntity, ct).Returns(userGroupEntity);
        Mapper.Map<UserGroupDto>(userGroupEntity).Returns(Fixture.Create<UserGroupDto>());

        //Act
        await Service.Create(auth0Id, createDto, ct);

        //Assert
        await UserRepository.Received(1).GetByAuth0Id(auth0Id, ct);
        await GroupRepository.Received(1).Create(Arg.Any<UserGroup>(), ct);
        await Mediator.Received(1).Publish(Arg.Any<GroupCreatedSignal>(), ct);
    }

    [Fact]
    public async Task UnshareSubscription_WhenDataIsValid_RemovesSubscription()
    {
        //Arrange
        var groupId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ct = CancellationToken.None;

        var subscription = Fixture.Build<Subscription>()
            .With(s => s.Id, subscriptionId)
            .Create();

        var userGroup = Fixture.Build<UserGroup>()
            .With(g => g.Id, groupId)
            .With(g => g.UserId, userId)
            .With(g => g.SharedSubscriptions, [subscription])
            .Create();

        var expectedDto = Fixture.Build<UserGroupDto>()
            .With(dto => dto.Id, groupId)
            .Create();

        GroupRepository.GetFullInfoById(groupId, ct).Returns(userGroup);
        GroupRepository.Update(userGroup, ct).Returns(userGroup);
        Mapper.Map<UserGroupDto>(userGroup).Returns(expectedDto);

        //Act
        var result = await Service.UnshareSubscription(groupId, subscriptionId, ct);

        //Assert
        result.ShouldNotBeNull();
        
        await GroupRepository.Received(1).Update(
            Arg.Is<UserGroup>(g => g.SharedSubscriptions != null && !g.SharedSubscriptions.Contains(subscription)), 
            ct);

        await Mediator.Received(1).Publish(
            Arg.Is<GroupUpdatedSignal>(s => s.GroupId == groupId && s.UserId == userId), 
            ct);
    }

    [Fact]
    public async Task UnshareSubscription_WhenGroupDoesNotExist_ThrowsUnknownIdentifierException()
    {
        //Arrange
        var groupId = Guid.NewGuid();
        var ct = CancellationToken.None;

        GroupRepository.GetFullInfoById(groupId, ct).Returns((UserGroup?)null);

        //Act & Assert
        await Should.ThrowAsync<UnknownIdentifierException>(async () =>
        {
            await Service.UnshareSubscription(groupId, Guid.NewGuid(), ct);
        });

        await GroupRepository.DidNotReceive().Update(Arg.Any<UserGroup>(), ct);
        await Mediator.DidNotReceive().Publish(Arg.Any<GroupUpdatedSignal>(), ct);
    }

    [Fact]
    public async Task UnshareSubscription_WhenSubscriptionNotShared_ThrowsArgumentException()
    {
        //Arrange
        var groupId = Guid.NewGuid();
        var ct = CancellationToken.None;

        var userGroup = Fixture.Build<UserGroup>()
            .With(g => g.Id, groupId)
            .With(g => g.SharedSubscriptions, []) 
            .Create();

        GroupRepository.GetFullInfoById(groupId, ct).Returns(userGroup);

        //Act & Assert
        await Should.ThrowAsync<ArgumentException>(async () =>
        {
            await Service.UnshareSubscription(groupId, Guid.NewGuid(), ct);
        });

        await GroupRepository.DidNotReceive().Update(Arg.Any<UserGroup>(), ct);
    }
    
    [Fact]
    public async Task Update_WhenValidModel_ReturnsUpdatedUserGroupDto()
    {
        //Arrange
        var updateId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ct = CancellationToken.None;

        var userGroupEntity = Fixture.Build<UserGroup>()
            .With(g => g.Id, updateId)
            .With(g => g.UserId, userId)
            .Create();

        var updateDto = Fixture.Build<UpdateUserGroupDto>()
            .With(dto => dto.Id, updateId)
            .Create();

        var userGroupDto = Fixture.Build<UserGroupDto>()
            .With(dto => dto.Id, updateId)
            .With(dto => dto.Name, updateDto.Name)
            .Create();

        GroupRepository.GetById(updateId, ct).Returns(userGroupEntity);
        GroupRepository.Update(userGroupEntity, ct).Returns(userGroupEntity);
    
        Mapper.Map(updateDto, userGroupEntity).Returns(userGroupEntity);
        Mapper.Map<UserGroupDto>(userGroupEntity).Returns(userGroupDto);

        //Act
        var result = await Service.Update(updateId, updateDto, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(updateId);
        result.Name.ShouldBe(updateDto.Name);

        await GroupRepository.Received(1).Update(userGroupEntity, ct);
    
        await Mediator.Received(1).Publish(Arg.Is<GroupUpdatedSignal>(s => 
            s.GroupId == updateId && 
            s.UserId == userId), ct);
    }

    [Fact]
    public async Task Update_WhenGivenUnknownId_ThrowsUnknownIdentifierException()
    {
        //Arrange
        var updateId = Guid.NewGuid();
        var updateDto = Fixture.Create<UpdateUserGroupDto>();
        var ct = CancellationToken.None;

        GroupRepository.GetById(updateId, ct).Returns((UserGroup?)null);

        //Act & Assert
        await Should.ThrowAsync<UnknownIdentifierException>(async () =>
        {
            await Service.Update(updateId, updateDto, ct);
        });

        await GroupRepository.DidNotReceive().Update(Arg.Any<UserGroup>(), ct);
        await Mediator.DidNotReceive().Publish(Arg.Any<GroupUpdatedSignal>(), ct);
    }
}
