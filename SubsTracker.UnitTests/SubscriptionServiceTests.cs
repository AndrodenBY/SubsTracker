using System.Linq.Expressions;
using AutoFixture;
using NSubstitute;
using Shouldly;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.RedisSettings;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
using SubsTracker.Messaging.Contracts;
using SubsTracker.UnitTests.TestsBase;

namespace SubsTracker.UnitTests;

public class SubscriptionServiceTests : SubscriptionServiceTestsBase
{
    [Fact]
    public async Task CancelSubscription_WhenValidModel_ReturnsInactiveSubscription()
{
    //Arrange
    var ct = CancellationToken.None;
    var auth0Id = "auth0|test-user";
    var userId = Guid.NewGuid();
    var subscriptionId = Guid.NewGuid();

    var userEntity = Fixture.Build<UserEntity>()
        .With(u => u.Id, userId)
        .With(u => u.Auth0Id, auth0Id)
        .Create();

    var subscriptionEntity = Fixture.Build<SubscriptionEntity>()
        .With(s => s.Id, subscriptionId)
        .With(s => s.UserId, userId)
        .With(s => s.Active, true)
        .Create();

    var updatedSubscriptionEntity = Fixture.Build<SubscriptionEntity>()
        .With(s => s.Id, subscriptionId)
        .With(s => s.UserId, userId)
        .With(s => s.Active, false)
        .Create();

    var updatedSubscriptionDto = Fixture.Build<SubscriptionDto>()
        .With(dto => dto.Id, subscriptionId)
        .Create();

    UserRepository.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>())
        .Returns(Task.FromResult<UserEntity?>(userEntity));

    SubscriptionRepository.GetById(subscriptionId, Arg.Any<CancellationToken>())
        .Returns(Task.FromResult<SubscriptionEntity?>(subscriptionEntity));

    SubscriptionRepository.Update(Arg.Any<SubscriptionEntity>(), Arg.Any<CancellationToken>())
        .Returns(Task.FromResult(updatedSubscriptionEntity));

    Mapper.Map<SubscriptionDto>(updatedSubscriptionEntity)
        .Returns(updatedSubscriptionDto);

    //Act
    var result = await Service.CancelSubscription(auth0Id, subscriptionId, ct);

    //Assert
    result.ShouldNotBeNull();
    result.Id.ShouldBe(subscriptionId);
    
    await SubscriptionRepository.Received(1).Update(
        Arg.Is<SubscriptionEntity>(s => s.Id == subscriptionId && !s.Active), 
        Arg.Any<CancellationToken>()
    );

    await MessageService.Received(1).NotifySubscriptionCanceled(
        Arg.Is<SubscriptionCanceledEvent>(e => e.Id == subscriptionId), 
        Arg.Any<CancellationToken>());
}
    [Fact]
    public async Task CancelSubscription_WhenIncorrectId_ReturnsNotFoundException()
    {
        //Arrange
        var emptyDto = new UpdateSubscriptionDto();

        //Act
        var result = async () => await Service.Update(Guid.Empty, emptyDto, Arg.Any<CancellationToken>());

        //Assert
        await result.ShouldThrowAsync<UnknownIdentifierException>();
    }

    [Fact]
    public async Task CancelSubscription_WhenSuccessful_InvalidatesSubscriptionAndBillsCache()
{
    //Arrange
    var ct = CancellationToken.None;
    var auth0Id = "auth0|123456789";
    var userId = Guid.NewGuid();
    var subscriptionId = Guid.NewGuid();
    
    var userEntity = Fixture.Build<UserEntity>()
        .With(u => u.Id, userId)
        .With(u => u.Auth0Id, auth0Id)
        .Create();
    
    var subscriptionEntity = Fixture.Build<SubscriptionEntity>()
        .With(s => s.Id, subscriptionId)
        .With(s => s.UserId, userId)
        .With(s => s.Active, true)
        .Create();
    
    var cancelledEntity = Fixture.Build<SubscriptionEntity>()
        .With(s => s.Id, subscriptionId)
        .With(s => s.UserId, userId)
        .With(s => s.Active, false)
        .Create();
    
    UserRepository.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>())
        .Returns(Task.FromResult<UserEntity?>(userEntity));
    
    SubscriptionRepository.GetById(subscriptionId, Arg.Any<CancellationToken>())
        .Returns(Task.FromResult<SubscriptionEntity?>(subscriptionEntity));
    
    SubscriptionRepository.Update(Arg.Any<SubscriptionEntity>(), Arg.Any<CancellationToken>())
        .Returns(Task.FromResult(cancelledEntity));
    
    Mapper.Map<SubscriptionDto>(cancelledEntity)
        .Returns(Fixture.Create<SubscriptionDto>());

    var subscriptionCacheKey = RedisKeySetter.SetCacheKey<SubscriptionDto>(subscriptionId);
    var billsCacheKey = RedisKeySetter.SetCacheKey(userId, "upcoming_bills");

    //Act
    await Service.CancelSubscription(auth0Id, subscriptionId, ct);

    //Assert
    await CacheAccessService.Received(1).RemoveData(
        Arg.Is<List<string>>(list =>
            list.Contains(subscriptionCacheKey) &&
            list.Contains(billsCacheKey)
        ),
        Arg.Any<CancellationToken>());
    
    await HistoryRepository.Received(1)
        .Create(subscriptionId, SubscriptionAction.Cancel, null, Arg.Any<CancellationToken>());
    
    await MessageService.Received(1)
        .NotifySubscriptionCanceled(Arg.Is<SubscriptionCanceledEvent>(e => e.Id == subscriptionId), Arg.Any<CancellationToken>());
}
    
    [Fact]
    public async Task Update_WhenValidModel_ReturnsUpdatedEntity()
    {
        //Arrange
        var ct = CancellationToken.None;
        var auth0Id = "auth0|test-user";
        var userId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();

        var userEntity = Fixture.Build<UserEntity>()
            .With(u => u.Id, userId)
            .Create();

        var subscriptionEntity = Fixture.Build<SubscriptionEntity>()
            .With(s => s.Id, subscriptionId)
            .With(s => s.UserId, userId)
            .Create();

        var updateDto = Fixture.Build<UpdateSubscriptionDto>()
            .With(dto => dto.Id, subscriptionId)
            .Create();

        var subscriptionDto = Fixture.Build<SubscriptionDto>()
            .With(dto => dto.Id, subscriptionId)
            .With(dto => dto.Name, updateDto.Name)
            .Create();
    
        UserRepository.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(userEntity));
    
        SubscriptionRepository.GetById(subscriptionId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SubscriptionEntity?>(subscriptionEntity));

        SubscriptionRepository.Update(Arg.Any<SubscriptionEntity>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(subscriptionEntity));

        Mapper.Map(updateDto, subscriptionEntity)
            .Returns(subscriptionEntity);

        Mapper.Map<SubscriptionDto>(subscriptionEntity)
            .Returns(subscriptionDto);

        //Act
        var result = await Service.Update(auth0Id, updateDto, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(subscriptionId);
        result.Name.ShouldBe(updateDto.Name);

        await UserRepository.Received(1).GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>());
        await SubscriptionRepository.Received(1).Update(
            Arg.Is<SubscriptionEntity>(s => s.Id == subscriptionId), 
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Update_WhenIncorrectId_ReturnsNotFoundException()
    {
        //Arrange
        var emptyDto = new UpdateSubscriptionDto();

        //Act
        var result = async () => await Service.Update(Guid.Empty, emptyDto, Arg.Any<CancellationToken>());

        //Assert
        await result.ShouldThrowAsync<UnknownIdentifierException>();
    }
    
    [Fact]
    public async Task RenewSubscription_WhenValidModel_ReturnsRenewSubscription()
    {
        //Arrange
        var ct = CancellationToken.None;
        var monthsToRenew = 2;
        var originalDueDate = new DateOnly(2025, 1, 15);
        var expectedDueDate = originalDueDate.AddMonths(monthsToRenew);

        var subscriptionEntity = Fixture.Build<SubscriptionEntity>()
            .With(s => s.DueDate, originalDueDate)
            .Create();

        var subscriptionDto = Fixture.Build<SubscriptionDto>()
            .With(s => s.Id, subscriptionEntity.Id)
            .With(s => s.Name, subscriptionEntity.Name)
            .With(s => s.Price, subscriptionEntity.Price)
            .With(s => s.DueDate, expectedDueDate)
            .With(s => s.Content, subscriptionEntity.Content)
            .With(s => s.Type, subscriptionEntity.Type)
            .Create();

        SubscriptionRepository.GetUserInfoById(subscriptionEntity.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SubscriptionEntity?>(subscriptionEntity));

        SubscriptionRepository.Update(Arg.Any<SubscriptionEntity>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(subscriptionEntity));

        Mapper.Map<SubscriptionDto>(subscriptionEntity)
            .Returns(subscriptionDto);

        //Act
        var result = await Service.RenewSubscription(subscriptionEntity.Id, monthsToRenew, ct);

        //Assert
        result.ShouldNotBeNull();
        result.DueDate.ShouldBe(expectedDueDate);

        await SubscriptionRepository.Received(1).Update(
            Arg.Is<SubscriptionEntity>(s => s.DueDate == expectedDueDate), 
            Arg.Any<CancellationToken>()
        );

        await MessageService.Received(1).NotifySubscriptionRenewed(
            Arg.Is<SubscriptionRenewedEvent>(e => e.Id == subscriptionDto.Id), 
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task RenewSubscription_WhenInvalidMonthsValue_ReturnsValidationException()
    {
        //Arrange
        var monthsToRenew = -1;
        var subscription = Fixture.Create<SubscriptionEntity>();

        SubscriptionRepository.GetUserInfoById(subscription.Id, Arg.Any<CancellationToken>())
            .Returns(subscription);

        //Act & Assert
        await Should.ThrowAsync<InvalidRequestDataException>(async () =>
            await Service.RenewSubscription(subscription.Id, monthsToRenew, Arg.Any<CancellationToken>()));
    }
    
    [Fact]
    public async Task GetUpcomingBills_WhenMultipleSubscriptionsAreDue_ReturnsAllUpcomingBills()
    {
        //Arrange
        var ct = CancellationToken.None;
        var auth0Id = "auth0|test-user";
        var existingUser = Fixture.Create<UserEntity>();
        var dueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
        var cacheKey = RedisKeySetter.SetCacheKey(existingUser.Id, "upcoming_bills");

        var subscriptions = Fixture.Build<SubscriptionEntity>()
            .With(s => s.UserId, existingUser.Id)
            .With(s => s.DueDate, dueDate)
            .CreateMany(3)
            .ToList();

        var subscriptionDtos = subscriptions.Select(s => Fixture.Build<SubscriptionDto>()
            .With(dto => dto.Id, s.Id)
            .With(dto => dto.DueDate, s.DueDate)
            .Create()).ToList();

        UserRepository.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(existingUser));

        CacheAccessService.GetData<List<SubscriptionDto>>(cacheKey, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<List<SubscriptionDto>?>(null));

        SubscriptionRepository.GetUpcomingBills(existingUser.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(subscriptions));

        Mapper.Map<List<SubscriptionDto>>(subscriptions)
            .Returns(subscriptionDtos);

        //Act
        var result = await Service.GetUpcomingBills(auth0Id, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result.First().Id.ShouldBe(subscriptionDtos.First().Id);

        await UserRepository.Received(1).GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>());
        await SubscriptionRepository.Received(1).GetUpcomingBills(existingUser.Id, Arg.Any<CancellationToken>());
    
        await CacheAccessService.Received(1).SetData(
            cacheKey,
            Arg.Is<List<SubscriptionDto>>(l => l.Count == 3),
            Arg.Any<TimeSpan>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetUpcomingBills_WhenSubscriptionsExistButNoneAreDueSoon_ReturnsEmptyCollection()
    {
        //Arrange
        var ct = CancellationToken.None;
        var auth0Id = "auth0|test-user";
        var existingUser = Fixture.Create<UserEntity>();
        var cacheKey = RedisKeySetter.SetCacheKey(existingUser.Id, "upcoming_bills");

        var futureDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
        var subscriptions = Fixture.Build<SubscriptionEntity>()
            .With(s => s.UserId, existingUser.Id)
            .With(s => s.DueDate, futureDueDate)
            .CreateMany(3)
            .ToList();

        UserRepository.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(existingUser));

        CacheAccessService.GetData<List<SubscriptionDto>>(cacheKey, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<List<SubscriptionDto>?>(null));

        SubscriptionRepository.GetUpcomingBills(existingUser.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(subscriptions));

        Mapper.Map<List<SubscriptionDto>>(subscriptions)
            .Returns([]);

        //Act
        var result = await Service.GetUpcomingBills(auth0Id, ct);

        //Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();

        await UserRepository.Received(1).GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>());
        await SubscriptionRepository.Received(1).GetUpcomingBills(existingUser.Id, Arg.Any<CancellationToken>());
        await CacheAccessService.Received(1).SetData(
            cacheKey, 
            Arg.Is<List<SubscriptionDto>>(l => l.Count == 0), 
            Arg.Any<TimeSpan>(), 
            Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task GetById_WhenSubscriptionExists_ReturnsSubscriptionDto()
    {
        //Arrange
        var ct = CancellationToken.None;
        var subscriptionEntity = Fixture.Create<SubscriptionEntity>();

        var subscriptionDto = Fixture.Build<SubscriptionDto>()
            .With(subscription => subscription.Id, subscriptionEntity.Id)
            .With(subscription => subscription.Name, subscriptionEntity.Name)
            .With(subscription => subscription.Price, subscriptionEntity.Price)
            .With(subscription => subscription.DueDate, subscriptionEntity.DueDate)
            .Create();

        var cacheKey = $"{subscriptionDto.Id}:{nameof(SubscriptionDto)}";

        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<SubscriptionDto?>>>(),
            Arg.Any<CancellationToken>()
        ).Returns(async callInfo =>
        {
            var factory = callInfo.Arg<Func<Task<SubscriptionDto?>>>();
            return await factory();
        });

        SubscriptionRepository.GetUserInfoById(subscriptionEntity.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SubscriptionEntity?>(subscriptionEntity));

        Mapper.Map<SubscriptionDto>(subscriptionEntity)
            .Returns(subscriptionDto);

        //Act
        var result = await Service.GetUserInfoById(subscriptionEntity.Id, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(subscriptionEntity.Id);
        result.Name.ShouldBe(subscriptionEntity.Name);
        result.Price.ShouldBe(subscriptionEntity.Price);
        result.DueDate.ShouldBe(subscriptionEntity.DueDate);

        await SubscriptionRepository.Received(1).GetUserInfoById(subscriptionEntity.Id, Arg.Any<CancellationToken>());
        await CacheService.Received(1).CacheDataWithLock(
            Arg.Is<string>(s => s == cacheKey),
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<SubscriptionDto?>>>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetById_WhenEmptyGuid_ThrowsNotFoundException()
    {
        //Arrange
        var emptyId = Guid.Empty;

        //Act
        var emptyIdResult = async() => await Service.GetById(emptyId, Arg.Any<CancellationToken>());

        //Assert
        await Should.ThrowAsync<UnknownIdentifierException>(emptyIdResult);
    }

    [Fact]
    public async Task GetById_WhenSubscriptionDoesNotExist_ThrowsNotFoundException()
    {
        //Arrange
        var fakeId = Guid.NewGuid();

        //Act
        var fakeIdResult = async () => await Service.GetById(fakeId, Arg.Any<CancellationToken>());

        //Assert
        await Should.ThrowAsync<UnknownIdentifierException>(fakeIdResult);
    }

    [Fact]
    public async Task GetById_WhenCacheHit_ReturnsCachedDataAndSkipsRepo()
    {
        //Arrange
        var ct = CancellationToken.None;
        var cachedDto = Fixture.Create<SubscriptionDto>();
        
        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<SubscriptionDto?>>>(),
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult<SubscriptionDto?>(cachedDto));

        //Act
        var result = await Service.GetById(cachedDto.Id, ct);

        //Assert
        result.ShouldNotBeNull();
        result.ShouldBe(cachedDto);

        await SubscriptionRepository.DidNotReceive().GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await CacheService.Received(1).CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<SubscriptionDto?>>>(),
            Arg.Any<CancellationToken>()
        );
    }
    
    [Fact]
    public async Task GetAll_WhenFilteredByName_ReturnsCorrectSubscription()
    {
        //Arrange
        var subscriptionToFind = Fixture.Create<SubscriptionEntity>();
        var subscriptionDto = Fixture.Build<SubscriptionDto>()
            .With(subscription => subscription.Id, subscriptionToFind.Id)
            .With(subscription => subscription.Name, subscriptionToFind.Name)
            .Create();

        var filter = new SubscriptionFilterDto { Name = subscriptionToFind.Name };

        SubscriptionRepository.GetAll(Arg.Any<Expression<Func<SubscriptionEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<SubscriptionEntity> { subscriptionToFind });
        Mapper.Map<List<SubscriptionDto>>(Arg.Any<List<SubscriptionEntity>>())
            .Returns(new List<SubscriptionDto> { subscriptionDto });

        //Act
        var result = await Service.GetAll(filter, Arg.Any<CancellationToken>());

        //Assert
        result.ShouldNotBeNull();
        result.ShouldHaveSingleItem();
        result.Single().Name.ShouldBe(subscriptionToFind.Name);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentName_ReturnsEmptyList()
    {
        //Arrange
        var subscriptionToFind = Fixture.Create<SubscriptionEntity>();
        Fixture.Build<SubscriptionDto>()
            .With(subscription => subscription.Id, subscriptionToFind.Id)
            .With(subscription => subscription.Name, subscriptionToFind.Name)
            .Create();

        var filter = new SubscriptionFilterDto { Name = "LetThatSinkIn" };

        SubscriptionRepository.GetAll(Arg.Any<Expression<Func<SubscriptionEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<SubscriptionEntity>());
        Mapper.Map<List<SubscriptionDto>>(Arg.Any<List<SubscriptionEntity>>()).Returns(new List<SubscriptionDto>());

        //Act
        var result = await Service.GetAll(filter, Arg.Any<CancellationToken>());

        //Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WhenNoSubscriptions_ReturnsEmptyList()
    {
        //Arrange
        var filter = new SubscriptionFilterDto();

        SubscriptionRepository.GetAll(Arg.Any<Expression<Func<SubscriptionEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<SubscriptionEntity>());
        Mapper.Map<List<SubscriptionDto>>(Arg.Any<List<SubscriptionEntity>>()).Returns(new List<SubscriptionDto>());

        //Act
        var result = await Service.GetAll(filter, Arg.Any<CancellationToken>());

        //Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WhenFilterIsEmpty_ReturnsAllSubscriptions()
    {
        //Arrange
        var ct = CancellationToken.None;
        var subscriptions = Fixture.CreateMany<SubscriptionEntity>(3).ToList();
        var subscriptionDtos = Fixture.CreateMany<SubscriptionDto>(3).ToList();

        var filter = new SubscriptionFilterDto();

        SubscriptionRepository.GetAll(Arg.Any<Expression<Func<SubscriptionEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(subscriptions));

        Mapper.Map<List<SubscriptionDto>>(subscriptions)
            .Returns(subscriptionDtos);

        //Act
        var result = await Service.GetAll(filter, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result.ShouldBe(subscriptionDtos);
    
        await SubscriptionRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<SubscriptionEntity, bool>>>(), 
            Arg.Any<CancellationToken>()
        );
    }
    
    [Fact]
    public async Task Create_WhenValidModel_ReturnsCreatedSubscription()
    {
        //Arrange
        var ct = CancellationToken.None;
        var auth0Id = "auth0|test-user";
        var createDto = Fixture.Create<CreateSubscriptionDto>();
        var existingUser = Fixture.Create<UserEntity>();

        var subscriptionEntity = Fixture.Build<SubscriptionEntity>()
            .With(s => s.Name, createDto.Name)
            .With(s => s.UserId, existingUser.Id)
            .Create();

        var subscriptionDto = Fixture.Build<SubscriptionDto>()
            .With(s => s.Name, createDto.Name)
            .Create();

        UserRepository.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(existingUser));

        SubscriptionRepository.GetByPredicate(Arg.Any<Expression<Func<SubscriptionEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SubscriptionEntity?>(null));

        Mapper.Map<SubscriptionEntity>(createDto)
            .Returns(subscriptionEntity);

        SubscriptionRepository.Create(subscriptionEntity, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(subscriptionEntity));

        Mapper.Map<SubscriptionDto>(subscriptionEntity)
            .Returns(subscriptionDto);

        //Act
        var result = await Service.Create(auth0Id, createDto, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(createDto.Name);

        await UserRepository.Received(1).GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>());

        await SubscriptionRepository.Received(1).Create(
            Arg.Is<SubscriptionEntity>(s => s.Name == createDto.Name && s.UserId == existingUser.Id), 
            Arg.Any<CancellationToken>());

        await HistoryRepository.Received(1).Create(
            subscriptionEntity.Id, 
            SubscriptionAction.Activate, 
            createDto.Price, 
            Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Create_WhenSubscriptionAlreadyExists_ThrowsPolicyViolationException()
    {
        //Arrange
        var ct = CancellationToken.None;
        var auth0Id = "auth0|test-user";
        var createDto = Fixture.Create<CreateSubscriptionDto>();
        var existingUser = Fixture.Create<UserEntity>();

        var existingSubscription = Fixture.Build<SubscriptionEntity>()
            .With(s => s.Name, createDto.Name)
            .With(s => s.UserId, existingUser.Id)
            .With(s => s.Active, true)
            .Create();

        UserRepository.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(existingUser));
    
        SubscriptionRepository.GetByPredicate(Arg.Any<Expression<Func<SubscriptionEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SubscriptionEntity?>(existingSubscription));

        //Act
        var act = async () => await Service.Create(auth0Id, createDto, ct);

        //Assert
        var exception = await act.ShouldThrowAsync<PolicyViolationException>();
        exception.Message.ShouldContain(createDto.Name);

        await SubscriptionRepository.DidNotReceive().Create(Arg.Any<SubscriptionEntity>(), Arg.Any<CancellationToken>());
        await HistoryRepository.DidNotReceive().Create(Arg.Any<Guid>(), Arg.Any<SubscriptionAction>(), Arg.Any<decimal?>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Create_WhenUserNotExists_ReturnsNull()
    {
        //Arrange
        var createDto = Fixture.Create<CreateSubscriptionDto>();

        //Act & Assert
        await Should.ThrowAsync<UnknownIdentifierException>(async () =>
            await Service.Create(string.Empty, createDto, Arg.Any<CancellationToken>()));
    }
}
