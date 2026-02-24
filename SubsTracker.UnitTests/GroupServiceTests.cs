using System.Linq.Expressions;
using AutoFixture;
using NSubstitute;
using Shouldly;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Mediator.Signals;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
using SubsTracker.Domain.Pagination;
using SubsTracker.UnitTests.TestsBase;

namespace SubsTracker.UnitTests;

public class GroupServiceTests : GroupServiceTestsBase
{
    [Fact]
    public async Task GetAll_WhenFilteredByName_ReturnsCorrectUserGroup()
    {
        //Arrange
        var ct = CancellationToken.None;
        var userGroupToFind = Fixture.Create<GroupEntity>();
        var groupDto = Fixture.Build<GroupDto>()
            .With(g => g.Name, userGroupToFind.Name)
            .Create();
        var filter = new GroupFilterDto { Name = userGroupToFind.Name };
    
        var pagedList = new PaginatedList<GroupEntity>([userGroupToFind], 1, 10, 1);

        GroupRepository.GetAll(
                Arg.Any<Expression<Func<GroupEntity, bool>>>(), 
                Arg.Any<PaginationParameters?>(),
                Arg.Is(ct))
            .Returns(pagedList);
        
        Mapper.Map<GroupDto>(Arg.Is<GroupEntity>(e => e.Id == userGroupToFind.Id))
            .Returns(groupDto);

        //Act 
        var result = await Service.GetAll(filter, null, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Items.ShouldHaveSingleItem();
        result.Items[0].Name.ShouldBe(userGroupToFind.Name); 

        await GroupRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<GroupEntity, bool>>>(),
            Arg.Any<PaginationParameters?>(),
            ct
        );
    }

    [Fact]
    public async Task GetAll_WhenFilteredByPartialName_ReturnsMatchingGroups()
    {
        //Arrange
        var ct = CancellationToken.None;
        var filter = new GroupFilterDto { Name = "Family" };

        List<GroupEntity> groups = 
        [ 
            Fixture.Build<GroupEntity>().With(g => g.Name, "My Family Plan").Create(),
            Fixture.Build<GroupEntity>().With(g => g.Name, "FamilyAccount").Create()
        ];
    
        var pagedList = new PaginatedList<GroupEntity>(groups, 1, 10, 2);
    
        GroupRepository.GetAll(
                Arg.Any<Expression<Func<GroupEntity, bool>>>(), 
                Arg.Any<PaginationParameters?>(), 
                Arg.Is(ct))
            .Returns(pagedList);
    
        List<GroupDto> expectedDtos = [.. Fixture.CreateMany<GroupDto>(2)];
    
        Mapper.Map<List<GroupDto>>(Arg.Any<List<GroupEntity>>())
            .Returns(expectedDtos);

        //Act
        var result = await Service.GetAll(filter, null, ct);

        //Assert
        result.Items.Count.ShouldBe(2);
    
        await GroupRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<GroupEntity, bool>>>(), 
            Arg.Any<PaginationParameters?>(), 
            ct);
    }
    
    [Fact]
    public async Task GetAll_WhenFilteredByNameWithDifferentCase_ReturnsMatchingGroup()
    {
        //Arrange
        var ct = CancellationToken.None;
        const string groupName = "NetflixPremium";
        var filter = new GroupFilterDto { Name = "nEtFlIx" };

        var entity = Fixture.Build<GroupEntity>().With(g => g.Name, groupName).Create();
        var dto = Fixture.Build<GroupDto>().With(d => d.Name, groupName).Create();
    
        var pagedList = new PaginatedList<GroupEntity>([entity], 1, 10, 1);

        GroupRepository.GetAll(
                Arg.Any<Expression<Func<GroupEntity, bool>>>(), 
                Arg.Any<PaginationParameters?>(), 
                Arg.Is(ct))
            .Returns(pagedList);
        
        Mapper.Map<GroupDto>(Arg.Is<GroupEntity>(e => e.Id == entity.Id))
            .Returns(dto);
    
        //Act
        var result = await Service.GetAll(filter, null, ct);

        //Assert
        result.Items.ShouldHaveSingleItem();
        result.Items[0].Name.ShouldBe(groupName);

        await GroupRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<GroupEntity, bool>>>(),
            Arg.Any<PaginationParameters?>(),
            ct
        );
    }
    
    [Fact]
    public async Task GetAll_WhenFilterIsEmpty_ReturnsAllUserGroups()
    {
        // Arrange
        var ct = CancellationToken.None;
        var filter = new GroupFilterDto();

        List<GroupEntity> userGroups = [.. Fixture.CreateMany<GroupEntity>(3)];
        List<GroupDto> userGroupDtos = [.. Fixture.CreateMany<GroupDto>(3)];
    
        var pagedList = new PaginatedList<GroupEntity>(userGroups, 1, 10, 3);
    
        GroupRepository.GetAll(
                Arg.Any<Expression<Func<GroupEntity, bool>>>(), 
                Arg.Any<PaginationParameters?>(), 
                Arg.Is(ct))
            .Returns(pagedList);
        
        Mapper.Map<GroupDto>(Arg.Any<GroupEntity>())
            .Returns(userGroupDtos[0], userGroupDtos[1], userGroupDtos[2]);

        //Act
        var result = await Service.GetAll(filter, null, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(3);
        result.Items.ShouldBe(userGroupDtos);

        await GroupRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<GroupEntity, bool>>>(),
            Arg.Any<PaginationParameters?>(),
            ct
        );
    }

    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentName_ReturnsEmptyList()
    {
        //Arrange
        var ct = CancellationToken.None;
        var filter = new GroupFilterDto { Name = "NonExistentName123" };
        
        var emptyPagedList = new PaginatedList<GroupEntity>([], 1, 10, 0);
        
        GroupRepository.GetAll(
                Arg.Any<Expression<Func<GroupEntity, bool>>>(), 
                Arg.Any<PaginationParameters?>(), 
                Arg.Is(ct))
            .Returns(emptyPagedList);
        
        Mapper.Map<List<GroupDto>>(Arg.Is<List<GroupEntity>>(l => l.Count == 0))
            .Returns([]);

        //Act
        var result = await Service.GetAll(filter, null, ct);

        //Assert
        result.Items.ShouldBeEmpty();
    
        await GroupRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<GroupEntity, bool>>>(),
            Arg.Any<PaginationParameters?>(),
            ct
        );
    }

    [Fact]
    public async Task GetAll_WhenNoUserGroups_ReturnsEmptyList()
    {
        //Arrange
        var ct = CancellationToken.None;
        var filter = new GroupFilterDto();
        
        var emptyPagedList = new PaginatedList<GroupEntity>([], 1, 10, 0);
        
        GroupRepository.GetAll(
                Arg.Any<Expression<Func<GroupEntity, bool>>>(), 
                Arg.Any<PaginationParameters?>(), 
                Arg.Is(ct))
            .Returns(emptyPagedList);
        
        Mapper.Map<List<GroupDto>>(Arg.Any<List<GroupEntity>>()).Returns([]);

        //Act
        var result = await Service.GetAll(filter, null, ct);

        //Assert
        result.Items.ShouldBeEmpty();
    
        await GroupRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<GroupEntity, bool>>>(),
            Arg.Any<PaginationParameters?>(),
            ct
        );
    }
    
    [Fact]
    public async Task ShareSubscription_WhenValidData_AddSubscriptionToGroupAndPublishesSignal()
{
    //Arrange
    var ct = CancellationToken.None;
    var groupId = Guid.NewGuid();
    var userId = Guid.NewGuid();
    var subscriptionId = Guid.NewGuid();
    
    var userGroup = Fixture.Build<GroupEntity>()
        .With(g => g.Id, groupId)
        .With(g => g.UserId, userId)
        .With(g => g.SharedSubscriptions, new List<SubscriptionEntity>())
        .Create();

    var subscription = Fixture.Build<SubscriptionEntity>()
        .With(s => s.Id, subscriptionId)
        .Create();

    var expectedDto = Fixture.Build<GroupDto>()
        .With(dto => dto.Id, groupId)
        .Create();

    GroupRepository.GetFullInfoById(groupId, ct).Returns(userGroup);
    SubscriptionRepository.GetById(subscriptionId, ct).Returns(subscription);
    
    GroupRepository.Update(userGroup, ct).Returns(userGroup);
    Mapper.Map<GroupDto>(userGroup).Returns(expectedDto);

    //Act
    var result = await Service.ShareSubscription(groupId, subscriptionId, ct);

    //Assert
    result.ShouldNotBeNull();
    userGroup.SharedSubscriptions?.ShouldContain(subscription);
    
    await GroupRepository.Received(1).Update(userGroup, ct);
    await Mediator.Received(1).Publish(
        Arg.Is<GroupSignals.Updated>(s => s.GroupId == groupId && s.UserId == userId), 
        ct);
}

    [Fact]
    public async Task ShareSubscription_WhenGroupDoesNotExist_ThrowsUnknownIdentifierException()
    {
        //Arrange
        var groupId = Guid.NewGuid();
        var subId = Guid.NewGuid();

        GroupRepository.GetFullInfoById(groupId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<GroupEntity?>(null));

        //Act
        var act = () => Service.ShareSubscription(groupId, subId, CancellationToken.None);

        //Assert
        await act.ShouldThrowAsync<UnknownIdentifierException>();
        await GroupRepository.DidNotReceiveWithAnyArgs().Update(Arg.Any<GroupEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_WhenUserGroupExists_ReturnsGroupDto()
    {
        //Arrange
        var userGroupDto = Fixture.Create<GroupDto>();
        var ct = CancellationToken.None;

        var userGroup = Fixture.Build<GroupEntity>()
            .With(x => x.Id, userGroupDto.Id)
            .With(x => x.Name, userGroupDto.Name)
            .Create();

        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<Func<Task<GroupDto?>>>(),
            Arg.Any<CancellationToken>()
        ).Returns(async callInfo =>
        {
            var factory = callInfo.Arg<Func<Task<GroupDto?>>>();
            return await factory();
        });

        GroupRepository.GetById(userGroupDto.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<GroupEntity?>(userGroup));

        Mapper.Map<GroupDto>(userGroup)
            .Returns(userGroupDto);

        //Act
        var result = await Service.GetById(userGroupDto.Id, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(userGroupDto.Id);
        result.Name.ShouldBe(userGroupDto.Name);
    
        await CacheService.Received(1).CacheDataWithLock(
            Arg.Is<string>(s => s.Contains(userGroupDto.Id.ToString())),
            Arg.Any<Func<Task<GroupDto?>>>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetById_WhenIdIsEmpty_ThrowsUnknownIdentifierException()
    {
        // Arrange
        var emptyId = Guid.Empty;
        var ct = CancellationToken.None;
        
        async Task Act() => await Service.GetById(emptyId, ct);

        // Assert
        await Should.ThrowAsync<UnknownIdentifierException>(Act);
    }

    [Fact]
    public async Task GetById_WhenUserGroupDoesNotExist_ThrowsNotFoundException()
    {
        //Arrange
        var fakeId = Guid.NewGuid();

        //Act
        async Task Act() => await Service.GetById(fakeId, Arg.Any<CancellationToken>());

        //Assert
        await Should.ThrowAsync<UnknownIdentifierException>(Act);
    }

    [Fact]
    public async Task GetById_WhenCalled_CallsRepositoryExactlyOnce()
    {
        //Arrange
        var userGroupDto = Fixture.Create<GroupDto>();
        var ct = CancellationToken.None;

        var userGroup = Fixture.Build<GroupEntity>()
            .With(x => x.Id, userGroupDto.Id)
            .With(x => x.Name, userGroupDto.Name)
            .Create();

        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<Func<Task<GroupDto?>>>(),
            Arg.Any<CancellationToken>()
        ).Returns(async callInfo =>
        {
            var factory = callInfo.Arg<Func<Task<GroupDto?>>>();
            return await factory();
        });

        GroupRepository.GetById(userGroupDto.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<GroupEntity?>(userGroup));

        Mapper.Map<GroupDto>(userGroup)
            .Returns(userGroupDto);

        //Act
        await Service.GetById(userGroupDto.Id, ct);

        //Assert
        await GroupRepository.Received(1).GetById(userGroup.Id, Arg.Any<CancellationToken>());
        Mapper.Received(1).Map<GroupDto>(userGroup);
    }

    [Fact]
    public async Task GetById_WhenCacheHit_ReturnsCachedDataAndSkipsRepo()
    {
        //Arrange
        var cachedDto = Fixture.Create<GroupDto>();
        var ct = CancellationToken.None;
        
        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<Func<Task<GroupDto?>>>(),
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult<GroupDto?>(cachedDto));

        //Act
        var result = await Service.GetById(cachedDto.Id, ct);

        //Assert
        result.ShouldNotBeNull();
        result.ShouldBe(cachedDto);
        await GroupRepository.DidNotReceive().GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Delete_WhenCorrectModel_DeletesUserGroup()
    {
        //Arrange
        var userGroupEntity = Fixture.Create<GroupEntity>();
        var ct = CancellationToken.None;

        GroupRepository.GetById(userGroupEntity.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<GroupEntity?>(userGroupEntity));

        GroupRepository.Delete(userGroupEntity, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        //Act
        var result = await Service.Delete(userGroupEntity.Id, ct);

        //Assert
        result.ShouldBeTrue();
        await GroupRepository.Received(1).Delete(userGroupEntity, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_WhenEmptyGuid_ThrowsNotFoundException()
    {
        //Arrange
        var emptyId = Guid.Empty;

        GroupRepository.GetById(emptyId, Arg.Any<CancellationToken>()).Returns((GroupEntity?)null);

        //Act
        var result = async () => await Service.Delete(emptyId, Arg.Any<CancellationToken>());

        //Assert
        await result.ShouldThrowAsync<UnknownIdentifierException>();
    }
    
    [Fact]
    public async Task Create_WhenCalled_ReturnsCreatedGroupDto()
    {
        //Arrange
        var createDto = Fixture.Create<CreateGroupDto>();
        var ct = CancellationToken.None;

        var userGroupEntity = Fixture.Build<GroupEntity>()
            .With(userGroup => userGroup.Name, createDto.Name)
            .With(userGroup => userGroup.UserId, createDto.UserId)
            .Create();

        var userGroupDto = Fixture.Build<GroupDto>()
            .With(userGroup => userGroup.Name, userGroupEntity.Name)
            .With(userGroup => userGroup.Id, userGroupEntity.Id)
            .Create();

        UserRepository.GetById(createDto.UserId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(new UserEntity { Id = createDto.UserId }));

        Mapper.Map<GroupEntity>(createDto)
            .Returns(userGroupEntity);

        GroupRepository.Create(Arg.Any<GroupEntity>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(userGroupEntity));

        Mapper.Map<GroupDto>(userGroupEntity)
            .Returns(userGroupDto);

        //Act
        var result = await Service.Create(createDto, ct);

        //Assert
        result.ShouldNotBeNull();
        result.ShouldBeEquivalentTo(userGroupDto);
        await GroupRepository.Received(1).Create(Arg.Any<GroupEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_WhenEmptyDto_ThrowsValidationException()
    {
        //Arrange
        var createDto = new CreateGroupDto { Name = string.Empty, UserId = Guid.Empty };

        //Act & Assert
        await Should.ThrowAsync<InvalidRequestDataException>(async () =>
        {
            await Service.Create(string.Empty, createDto, Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Create_WhenCalled_CallsRepositoryExactlyOnce()
    {
        //Arrange
        var createDto = Fixture.Create<CreateGroupDto>();
        var ct = CancellationToken.None;
        
        var userGroupEntity = Fixture.Build<GroupEntity>()
            .With(userGroup => userGroup.Name, createDto.Name)
            .With(userGroup => userGroup.UserId, createDto.UserId)
            .Create();

        var userGroupDto = Fixture.Build<GroupDto>()
            .With(userGroup => userGroup.Name, userGroupEntity.Name)
            .With(userGroup => userGroup.Id, userGroupEntity.Id)
            .Create();
        
        UserRepository.GetById(createDto.UserId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(new UserEntity { Id = createDto.UserId }));
        Mapper.Map<GroupEntity>(createDto)
            .Returns(userGroupEntity);
        GroupRepository.Create(userGroupEntity, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(userGroupEntity));
        Mapper.Map<GroupDto>(userGroupEntity)
            .Returns(userGroupDto);

        //Act
        await Service.Create(createDto, ct);

        //Assert
        await GroupRepository.Received(1).Create(
            Arg.Is<GroupEntity>(ge => ge.Name == createDto.Name), 
            Arg.Any<CancellationToken>()
        );
        Mapper.Received(1).Map<GroupEntity>(createDto);
    }
    
    [Fact]
    public async Task UnshareSubscription_WhenDataIsValid_RemovesSubscriptionAndPublishesSignal()
{
    //Arrange
    var ct = CancellationToken.None;
    var groupId = Guid.NewGuid();
    var userId = Guid.NewGuid();
    var subscriptionId = Guid.NewGuid();

    var subscription = new SubscriptionEntity
    {
        Id = subscriptionId, 
        Type = SubscriptionType.Free, 
        Content = SubscriptionContent.Design,
        DueDate = DateOnly.MinValue, 
        Price = 9.99m
    };

    var userGroup = Fixture.Build<GroupEntity>()
        .With(g => g.Id, groupId)
        .With(g => g.UserId, userId)
        .With(g => g.SharedSubscriptions, [subscription]) 
        .Create();

    var expectedDto = Fixture.Build<GroupDto>()
        .With(dto => dto.Id, groupId)
        .Create();

    GroupRepository.GetFullInfoById(groupId, ct).Returns(userGroup);
    GroupRepository.Update(userGroup, ct).Returns(userGroup);
    Mapper.Map<GroupDto>(userGroup).Returns(expectedDto);

    //Act
    var result = await Service.UnshareSubscription(groupId, subscriptionId, ct);

    //Assert
    result.ShouldNotBeNull();
    userGroup.SharedSubscriptions.ShouldBeEmpty();

    await GroupRepository.Received(1).Update(userGroup, ct);
    await Mediator.Received(1).Publish(
        Arg.Is<GroupSignals.Updated>(s => s.GroupId == groupId && s.UserId == userId), 
        ct);
}

    [Fact]
    public async Task UnshareSubscription_WhenGroupDoesNotExist_ThrowsUnknownIdentifierException()
{
    //Arrange
    var groupId = Guid.NewGuid();
    
    GroupRepository.GetFullInfoById(groupId, Arg.Any<CancellationToken>())
        .Returns(Task.FromResult<GroupEntity?>(null));

    //Act
    var act = () => Service.UnshareSubscription(groupId, Guid.NewGuid(), CancellationToken.None);

    //Assert
    await act.ShouldThrowAsync<UnknownIdentifierException>();
}
    
    [Fact]
    public async Task Update_WhenValidModel_ReturnsUpdatedGroupDtoAndPublishesSignal()
{
    //Arrange
    var ct = CancellationToken.None;
    var groupId = Guid.NewGuid();
    var userId = Guid.NewGuid();

    var userGroupEntity = Fixture.Build<GroupEntity>()
        .With(g => g.Id, groupId)
        .With(g => g.UserId, userId)
        .Create();

    var updateDto = Fixture.Build<UpdateGroupDto>()
        .With(dto => dto.Id, groupId)
        .Create();
    
    var userGroupDto = Fixture.Build<GroupDto>()
        .With(dto => dto.Id, groupId)
        .With(dto => dto.Name, updateDto.Name)
        .Create();

    GroupRepository.GetById(groupId, ct)
        .Returns(userGroupEntity);

    GroupRepository.Update(userGroupEntity, ct)
        .Returns(userGroupEntity);
    
    Mapper.When(x => x.Map(updateDto, userGroupEntity))
          .Do(_ => userGroupEntity.Name = updateDto.Name!);

    Mapper.Map<GroupDto>(userGroupEntity)
        .Returns(userGroupDto);

    //Act
    var result = await Service.Update(groupId, updateDto, ct);

    //Assert
    result.ShouldNotBeNull();
    result.Id.ShouldBe(groupId);
    result.Name.ShouldBe(updateDto.Name);

    await GroupRepository.Received(1).Update(userGroupEntity, ct);
    await Mediator.Received(1).Publish(
        Arg.Is<GroupSignals.Updated>(s => s.GroupId == groupId && s.UserId == userId), 
        ct);
}

    [Fact]
    public async Task Update_WhenGroupDoesNotExist_ThrowsUnknownIdentifierException()
{
    //Arrange
    var updateId = Guid.NewGuid();
    var updateDto = Fixture.Create<UpdateGroupDto>();

    GroupRepository.GetById(updateId, Arg.Any<CancellationToken>())
        .Returns(Task.FromResult<GroupEntity?>(null));

    //Act
    var act = () => Service.Update(updateId, updateDto, CancellationToken.None);

    //Assert
    await act.ShouldThrowAsync<UnknownIdentifierException>();
    await GroupRepository.DidNotReceiveWithAnyArgs().Update(Arg.Any<GroupEntity>(), Arg.Any<CancellationToken>());
    await Mediator.DidNotReceiveWithAnyArgs().Publish(Arg.Any<GroupSignals.Updated>(), Arg.Any<CancellationToken>());
}
}
