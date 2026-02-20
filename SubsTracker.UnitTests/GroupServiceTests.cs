using System.Linq.Expressions;
using AutoFixture;
using NSubstitute;
using Shouldly;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.RedisSettings;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
using SubsTracker.UnitTests.TestsBase;

namespace SubsTracker.UnitTests;

public class GroupServiceTests : GroupServiceTestsBase
{
    [Fact]
    public async Task GetAll_WhenFilteredByName_ReturnsCorrectUserGroup()
    {
        //Arrange
        var userGroupToFind = Fixture.Create<GroupEntity>();
        var userGroupDto = Fixture.Build<GroupDto>()
            .With(userGroup => userGroup.Name, userGroupToFind.Name)
            .With(userGroup => userGroup.Id, userGroupToFind.Id)
            .Create();

        var filter = new GroupFilterDto { Name = userGroupToFind.Name };
        var ct = CancellationToken.None;

        GroupRepository.GetAll(Arg.Any<Expression<Func<GroupEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<List<GroupEntity>>([userGroupToFind]));

        Mapper.Map<List<GroupDto>>(Arg.Any<List<GroupEntity>>())
            .Returns([userGroupDto]);

        //Act
        var result = await Service.GetAll(filter, ct);

        //Assert
        result.ShouldNotBeNull();
        result.ShouldHaveSingleItem();
        result.Single().Name.ShouldBe(userGroupToFind.Name);
        await GroupRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<GroupEntity, bool>>>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetAll_WhenFilterIsEmpty_ReturnsAllUserGroups()
    {
        //Arrange
        var userGroups = Fixture.CreateMany<GroupEntity>(3).ToList();
        var userGroupDtos = Fixture.CreateMany<GroupDto>(3).ToList();

        var filter = new GroupFilterDto();
        var ct = CancellationToken.None;

        GroupRepository.GetAll(Arg.Any<Expression<Func<GroupEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(userGroups));

        Mapper.Map<List<GroupDto>>(userGroups)
            .Returns(userGroupDtos);

        //Act
        var result = await Service.GetAll(filter, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result.ShouldBe(userGroupDtos);
        await GroupRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<GroupEntity, bool>>>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentName_ReturnsEmptyList()
    {
        //Arrange
        var userGroupToFind = Fixture.Create<GroupEntity>();
        Fixture.Build<GroupDto>()
            .With(userGroup => userGroup.Name, userGroupToFind.Name)
            .With(userGroup => userGroup.Id, userGroupToFind.Id)
            .Create();

        var filter = new GroupFilterDto { Name = "Pv$$YbR3aK3rS123" };

        GroupRepository.GetAll(Arg.Any<Expression<Func<GroupEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<GroupEntity>());
        Mapper.Map<List<GroupDto>>(Arg.Any<List<GroupEntity>>()).Returns(new List<GroupDto>());

        //Act
        var result = await Service.GetAll(filter, Arg.Any<CancellationToken>());

        //Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WhenNoUserGroups_ReturnsEmptyList()
    {
        //Arrange
        var filter = new GroupFilterDto();

        GroupRepository.GetAll(Arg.Any<Expression<Func<GroupEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<GroupEntity>());
        Mapper.Map<List<GroupDto>>(Arg.Any<List<GroupEntity>>()).Returns(new List<GroupDto>());

        //Act
        var result = await Service.GetAll(filter, Arg.Any<CancellationToken>());

        //Assert
        result.ShouldBeEmpty();
    }
    
    [Fact]
    public async Task ShareSubscription_WhenValidData_AddSubscriptionToGroup()
    {
        //Arrange
        var ct = CancellationToken.None;
        var userGroup = Fixture.Build<GroupEntity>()
            .With(group => group.SharedSubscriptions, new List<SubscriptionEntity>())
            .Create();

        var subscription = new SubscriptionEntity
        {
            Id = Guid.NewGuid(), 
            Price = 9.99m, 
            Content = SubscriptionContent.Design, 
            DueDate = DateOnly.MaxValue,
            Type = SubscriptionType.Free
        };

        var expectedDto = Fixture.Build<GroupDto>()
            .With(group => group.Id, userGroup.Id)
            .With(group => group.Name, userGroup.Name)
            .Create();

        GroupRepository.GetFullInfoById(userGroup.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<GroupEntity?>(userGroup));

        SubscriptionRepository.GetById(subscription.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SubscriptionEntity?>(subscription));

        GroupRepository.Update(Arg.Any<GroupEntity>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(userGroup));

        Mapper.Map<GroupDto>(Arg.Any<GroupEntity>())
            .Returns(expectedDto);

        //Act
        var result = await Service.ShareSubscription(userGroup.Id, subscription.Id, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(userGroup.Id);
    
        await GroupRepository.Received(1).Update(
            Arg.Is<GroupEntity>(g => g.SharedSubscriptions != null && g.SharedSubscriptions.Contains(subscription)), 
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task ShareSubscription_WhenGroupDoesNotExist_ThrowsNotFoundException()
    {
        //Arrange
        var nonExistentGroupId = Guid.NewGuid();

        GroupRepository.GetFullInfoById(nonExistentGroupId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<GroupEntity?>(null));

        //Act
        var result = async () => await Service.ShareSubscription(nonExistentGroupId, Guid.NewGuid(), Arg.Any<CancellationToken>());

        //Assert
        await result.ShouldThrowAsync<UnknownIdentifierException>();
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

        var cacheKey = $"{userGroupDto.Id}:{nameof(GroupEntity)}";

        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<TimeSpan>(),
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
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<GroupDto?>>>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetById_WhenEmptyGuid_ThrowsNotFoundException()
    {
        //Arrange
        var emptyId = Guid.Empty;

        //Act
        var emptyIdResult = async () => await Service.GetById(emptyId, Arg.Any<CancellationToken>());

        //Assert
        await Should.ThrowAsync<UnknownIdentifierException>(emptyIdResult);
    }

    [Fact]
    public async Task GetById_WhenUserGroupDoesNotExist_ThrowsNotFoundException()
    {
        //Arrange
        var fakeId = Guid.NewGuid();

        //Act
        var fakeIdResult = async () => await Service.GetById(fakeId, Arg.Any<CancellationToken>());

        //Assert
        await Should.ThrowAsync<UnknownIdentifierException>(fakeIdResult);
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
            Arg.Any<TimeSpan>(),
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
            Arg.Any<TimeSpan>(),
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
    public async Task UnshareSubscription_WhenDataIsValid_RemovesSubscription()
    {
        //Arrange
        var ct = CancellationToken.None;
        var subscription = new SubscriptionEntity
        {
            Id = Guid.NewGuid(), 
            Type = SubscriptionType.Free, 
            Content = SubscriptionContent.Design,
            DueDate = DateOnly.MinValue, 
            Price = 9.99m
        };

        var userGroup = Fixture.Build<GroupEntity>()
            .With(group => group.SharedSubscriptions, new List<SubscriptionEntity> { subscription })
            .Create();

        var expectedDto = Fixture.Build<GroupDto>()
            .With(group => group.Id, userGroup.Id)
            .With(group => group.Name, userGroup.Name)
            .Create();

        GroupRepository.GetFullInfoById(userGroup.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<GroupEntity?>(userGroup));

        GroupRepository.Update(Arg.Any<GroupEntity>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(userGroup));

        Mapper.Map<GroupDto>(Arg.Any<GroupEntity>())
            .Returns(expectedDto);

        //Act
        var result = await Service.UnshareSubscription(userGroup.Id, subscription.Id, ct);

        //Assert
        result.ShouldNotBeNull();
        await GroupRepository.Received(1).Update(
            Arg.Is<GroupEntity>(g => g.SharedSubscriptions != null && !g.SharedSubscriptions.Contains(subscription)), 
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task UnshareSubscription_WhenGroupDoesNotExist_ThrowsNotFoundException()
    {
        //Arrange
        GroupRepository.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((GroupEntity?)null);

        //Act
        var result = async () => await Service.UnshareSubscription(Guid.NewGuid(), Guid.NewGuid(), Arg.Any<CancellationToken>());

        //Assert
        await result.ShouldThrowAsync<UnknownIdentifierException>();
    }
    
    [Fact]
    public async Task Update_WhenValidModel_ReturnsUpdatedGroupDto()
    {
        //Arrange
        var ct = CancellationToken.None;
        var userGroupEntity = Fixture.Create<GroupEntity>();
    
        var updateDto = Fixture.Build<UpdateGroupDto>()
            .With(userGroup => userGroup.Id, userGroupEntity.Id)
            .Create();
        
        var userGroupDto = Fixture.Build<GroupDto>()
            .With(userGroup => userGroup.Name, updateDto.Name)
            .With(userGroup => userGroup.Id, updateDto.Id)
            .Create();

        GroupRepository.GetById(updateDto.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<GroupEntity?>(userGroupEntity));

        GroupRepository.Update(Arg.Any<GroupEntity>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(userGroupEntity));

        Mapper.Map(updateDto, userGroupEntity)
            .Returns(userGroupEntity);

        Mapper.Map<GroupDto>(userGroupEntity)
            .Returns(userGroupDto);

        //Act
        var result = await Service.Update(updateDto.Id, updateDto, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(userGroupEntity.Id);
        result.Name.ShouldBe(updateDto.Name);
        await GroupRepository.Received(1).Update(Arg.Any<GroupEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_WhenGivenEmptyModel_ReturnsNotFoundException()
    {
        //Arrange
        var emptyDto = new UpdateGroupDto();

        //Act
        var result = async () => await Service.Update(Guid.Empty, emptyDto, Arg.Any<CancellationToken>());

        //Assert
        await result.ShouldThrowAsync<UnknownIdentifierException>();
    }
}
