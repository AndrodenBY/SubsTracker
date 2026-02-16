using SubsTracker.Domain.Pagination;

namespace SubsTracker.UnitTests.SubscriptionService;

public class SubscriptionServiceTests : SubscriptionServiceTestsBase
{
    [Fact]
    public async Task CancelSubscription_WhenValidModel_ReturnsInactiveSubscription()
    {
        //Arrange
        var auth0Id = Fixture.Create<string>();
        var userId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();

        var userEntity = Fixture.Build<User>()
            .With(u => u.Id, userId)
            .Create();

        var subscriptionEntity = Fixture.Build<Subscription>()
            .With(s => s.Id, subscriptionId)
            .With(s => s.UserId, userId)
            .With(s => s.Active, true)
            .Create();

        var updatedSubscriptionEntity = Fixture.Build<Subscription>()
            .With(s => s.Id, subscriptionId)
            .With(s => s.UserId, userId)
            .With(s => s.Active, false)
            .Create();

        var updatedSubscriptionDto = Fixture.Build<SubscriptionDto>()
            .With(dto => dto.Id, subscriptionId)
            .Create();

        UserRepository.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>())
            .Returns(userEntity);

        SubscriptionRepository.GetById(subscriptionId, Arg.Any<CancellationToken>())
            .Returns(subscriptionEntity);

        SubscriptionRepository.Update(Arg.Any<Subscription>(), Arg.Any<CancellationToken>())
            .Returns(updatedSubscriptionEntity);

        Mapper.Map<SubscriptionDto>(updatedSubscriptionEntity)
            .Returns(updatedSubscriptionDto);

        //Act
        var result = await Service.CancelSubscription(auth0Id, subscriptionId, default);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(subscriptionId);
        await SubscriptionRepository.Received(1)
            .Update(Arg.Is<Subscription>(s => s.Id == subscriptionId && s.Active == false), default);

        await SubscriptionRepository.Received(1)
            .Update(Arg.Is<Subscription>(s => s.Id == subscriptionId && !s.Active), Arg.Any<CancellationToken>());

        await MessageService.Received(1).NotifySubscriptionCanceled(
            Arg.Is<SubscriptionCanceledEvent>(e => e.Id == subscriptionId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelSubscription_WhenIncorrectId_ReturnsNotFoundException()
    {
        //Arrange
        var emptyDto = new UpdateSubscriptionDto();

        //Act
        var result = async () => await Service.Update(Guid.Empty, emptyDto, default);

        //Assert
        await result.ShouldThrowAsync<UnknownIdentifierException>();
    }

    [Fact]
    public async Task CancelSubscription_WhenSuccessful_InvalidatesSubscriptionAndBillsCache()
    {
        //Arrange
        var auth0Id = "auth0|123456789";
        var userId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        
        var userEntity = Fixture.Build<User>()
            .With(u => u.Id, userId)
            .With(u => u.Auth0Id, auth0Id)
            .Create();
        
        var subscriptionEntity = Fixture.Build<Subscription>()
            .With(s => s.Id, subscriptionId)
            .With(s => s.UserId, userId)
            .With(s => s.Active, true)
            .Create();
        
        var cancelledEntity = Fixture.Build<Subscription>()
            .With(s => s.Id, subscriptionId)
            .With(s => s.UserId, userId)
            .With(s => s.Active, false)
            .Create();
        
        UserRepository.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>()).Returns(userEntity);
        
        SubscriptionRepository.GetById(subscriptionId, Arg.Any<CancellationToken>()).Returns(subscriptionEntity);
        
        SubscriptionRepository.Update(Arg.Any<Subscription>(), Arg.Any<CancellationToken>()).Returns(cancelledEntity);
        
        Mapper.Map<SubscriptionDto>(cancelledEntity).Returns(Fixture.Create<SubscriptionDto>());

        var subscriptionCacheKey = RedisKeySetter.SetCacheKey<SubscriptionDto>(subscriptionId);
        var billsCacheKey = RedisKeySetter.SetCacheKey(userId, "upcoming_bills");

        //Act
        await Service.CancelSubscription(auth0Id, subscriptionId, default);

        //Assert
        await CacheAccessService.Received(1).RemoveData(
            Arg.Is<List<string>>(list =>
                list.Contains(subscriptionCacheKey) &&
                list.Contains(billsCacheKey) &&
                list.Count == 2
            ),
            Arg.Any<CancellationToken>());
        
        await HistoryRepository.Received(1)
            .Create(subscriptionId, SubscriptionAction.Cancel, null, Arg.Any<CancellationToken>());
        
        await MessageService.Received(1)
            .NotifySubscriptionCanceled(Arg.Any<SubscriptionCanceledEvent>(), Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task Update_WhenValidModel_ReturnsUpdatedEntity()
    {
        //Arrange
        var auth0Id = Fixture.Create<string>();
        var userId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();

        var userEntity = Fixture.Build<User>()
            .With(u => u.Id, userId)
            .Create();

        var subscriptionEntity = Fixture.Build<Subscription>()
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
            .Returns(userEntity);
        
        SubscriptionRepository.GetById(subscriptionId, Arg.Any<CancellationToken>())
            .Returns(subscriptionEntity);

        SubscriptionRepository.Update(Arg.Any<Subscription>(), Arg.Any<CancellationToken>())
            .Returns(subscriptionEntity);

        Mapper.Map<SubscriptionDto>(subscriptionEntity)
            .Returns(subscriptionDto);

        //Act
        var result = await Service.Update(auth0Id, updateDto, default);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(subscriptionId);
        result.Name.ShouldBe(updateDto.Name);

        await UserRepository.Received(1).GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>());
        await SubscriptionRepository.Received(1).Update(Arg.Is<Subscription>(s => s.Id == subscriptionId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_WhenIncorrectId_ReturnsNotFoundException()
    {
        //Arrange
        var emptyDto = new UpdateSubscriptionDto();

        //Act
        var result = async () => await Service.Update(Guid.Empty, emptyDto, default);

        //Assert
        await result.ShouldThrowAsync<UnknownIdentifierException>();
    }
    
    [Fact]
    public async Task RenewSubscription_WhenValidModel_ReturnsRenewSubscription()
    {
        //Arrange
        var monthsToRenew = 2;
        var originalDueDate = new DateOnly(2025, 1, 15);
        var subscriptionEntity = Fixture.Build<Subscription>()
            .With(subscription => subscription.DueDate, originalDueDate)
            .Create();
        var subscriptionDto = Fixture.Build<SubscriptionDto>()
            .With(s => s.Id, subscriptionEntity.Id)
            .With(s => s.Name, subscriptionEntity.Name)
            .With(s => s.Price, subscriptionEntity.Price)
            .With(s => s.DueDate, originalDueDate.AddMonths(monthsToRenew))
            .With(s => s.Content, subscriptionEntity.Content)
            .With(s => s.Type, subscriptionEntity.Type)
            .Create();

        SubscriptionRepository.GetUserInfoById(Arg.Any<Guid>(), default).Returns(subscriptionEntity);
        SubscriptionRepository.Update(Arg.Any<Subscription>(), default).Returns(subscriptionEntity);
        Mapper.Map<SubscriptionDto>(subscriptionEntity).Returns(subscriptionDto);

        //Act
        var result = await Service.RenewSubscription(subscriptionEntity.Id, monthsToRenew, default);

        //Assert
        result.ShouldNotBeNull();
        result.DueDate.ShouldBe(originalDueDate.AddMonths(monthsToRenew));
        await SubscriptionRepository.Received(1)
            .Update(Arg.Is<Subscription>(s => s.DueDate == originalDueDate.AddMonths(monthsToRenew)), default);
        await MessageService.Received(1).NotifySubscriptionRenewed(
            Arg.Is<SubscriptionRenewedEvent>(subscriptionCanceledEvent =>
                subscriptionCanceledEvent.Id == subscriptionDto.Id), default);
    }

    [Fact]
    public async Task RenewSubscription_WhenInvalidMonthsValue_ReturnsValidationException()
    {
        //Arrange
        var monthsToRenew = -1;
        var subscription = Fixture.Create<Subscription>();

        SubscriptionRepository.GetUserInfoById(subscription.Id, default)
            .Returns(subscription);

        //Act & Assert
        await Should.ThrowAsync<InvalidRequestDataException>(async () =>
            await Service.RenewSubscription(subscription.Id, monthsToRenew, default));
    }
    
    [Fact]
    public async Task GetUpcomingBills_WhenMultipleSubscriptionsAreDue_ReturnsAllUpcomingBills()
    {
        //Arrange
        var auth0Id = Fixture.Create<string>();
        var existingUser = Fixture.Create<User>();
        var dueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
        var cacheKey = RedisKeySetter.SetCacheKey(existingUser.Id, "upcoming_bills");

        var subscriptions = Fixture.Build<Subscription>()
            .With(s => s.UserId, existingUser.Id)
            .With(s => s.DueDate, dueDate)
            .CreateMany(3)
            .ToList();

        var subscriptionDtos = subscriptions.Select(s => Fixture.Build<SubscriptionDto>()
            .With(dto => dto.Id, s.Id)
            .With(dto => dto.DueDate, s.DueDate)
            .Create()).ToList();

        UserRepository.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>())
            .Returns(existingUser);

        CacheAccessService.GetData<List<SubscriptionDto>>(cacheKey, Arg.Any<CancellationToken>())
            .Returns((List<SubscriptionDto?>)null);

        SubscriptionRepository.GetUpcomingBills(existingUser.Id, Arg.Any<CancellationToken>())
            .Returns(subscriptions);

        Mapper.Map<List<SubscriptionDto>>(subscriptions)
            .Returns(subscriptionDtos);

        //Act
        var result = await Service.GetUpcomingBills(auth0Id, default);

        //Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result.First().Id.ShouldBe(subscriptionDtos.First().Id);

        await UserRepository.Received(1).GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>());
        await SubscriptionRepository.Received(1).GetUpcomingBills(existingUser.Id, Arg.Any<CancellationToken>());
        await CacheAccessService.Received(1).SetData(
            cacheKey,
            Arg.Is<List<SubscriptionDto>>(l => l.Count == 3),
            RedisConstants.ExpirationTime,
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetUpcomingBills_WhenSubscriptionsExistButNoneAreDueSoon_ReturnsEmptyCollection()
    {
        //Arrange
        var auth0Id = Fixture.Create<string>();
        var existingUser = Fixture.Create<User>();
        var cacheKey = RedisKeySetter.SetCacheKey(existingUser.Id, "upcoming_bills");

        var futureDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
        var subscriptions = Fixture.Build<Subscription>()
            .With(s => s.UserId, existingUser.Id)
            .With(s => s.DueDate, futureDueDate)
            .CreateMany(3)
            .ToList();

        UserRepository.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>())
            .Returns(existingUser);

        CacheAccessService.GetData<List<SubscriptionDto>>(cacheKey, Arg.Any<CancellationToken>())
            .Returns((List<SubscriptionDto?>)null);

        SubscriptionRepository.GetUpcomingBills(existingUser.Id, Arg.Any<CancellationToken>())
            .Returns(subscriptions);

        Mapper.Map<List<SubscriptionDto>>(subscriptions)
            .Returns(new List<SubscriptionDto>());

        //Act
        var result = await Service.GetUpcomingBills(auth0Id, default);

        //Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();

        await UserRepository.Received(1).GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>());
        await SubscriptionRepository.Received(1).GetUpcomingBills(existingUser.Id, Arg.Any<CancellationToken>());
        await CacheAccessService.Received(1).SetData(cacheKey, Arg.Is<List<SubscriptionDto>>(l => l.Count == 0), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task GetById_WhenSubscriptionExists_ReturnsSubscriptionDto()
    {
        //Arrange
        var subscriptionEntity = Fixture.Create<Subscription>();

        var subscriptionDto = Fixture.Build<SubscriptionDto>()
            .With(subscription => subscription.Id, subscriptionEntity.Id)
            .With(subscription => subscription.Name, subscriptionEntity.Name)
            .With(subscription => subscription.Price, subscriptionEntity.Price)
            .With(subscription => subscription.DueDate, subscriptionEntity.DueDate)
            .Create();

        var cacheKey = $"{subscriptionDto.Id}:{nameof(SubscriptionDto)}";

        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<Func<Task<SubscriptionDto?>>>(),
            default
        )!.Returns(callInfo =>
        {
            var factory = callInfo.Arg<Func<Task<SubscriptionDto>>>();
            return factory();
        });
        SubscriptionRepository.GetUserInfoById(subscriptionEntity.Id, default)
            .Returns(subscriptionEntity);

        Mapper.Map<SubscriptionDto>(subscriptionEntity)
            .Returns(subscriptionDto);

        //Act
        var result = await Service.GetUserInfoById(subscriptionEntity.Id, default);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(subscriptionEntity.Id);
        result.Name.ShouldBe(subscriptionEntity.Name);
        result.Price.ShouldBe(subscriptionEntity.Price);
        result.DueDate.ShouldBe(subscriptionEntity.DueDate);

        await SubscriptionRepository.Received(1).GetUserInfoById(subscriptionEntity.Id, default);
        await CacheService.Received(1).CacheDataWithLock(
            cacheKey,
            Arg.Any<Func<Task<SubscriptionDto?>>>(),
            default
        );
    }

    [Fact]
    public async Task GetById_WhenEmptyGuid_ThrowsNotFoundException()
    {
        //Arrange
        var emptyId = Guid.Empty;

        //Act
        var emptyIdResult = async() => await Service.GetById(emptyId, default);

        //Assert
        await Should.ThrowAsync<UnknownIdentifierException>(emptyIdResult);
    }

    [Fact]
    public async Task GetById_WhenSubscriptionDoesNotExist_ThrowsNotFoundException()
    {
        //Arrange
        var fakeId = Guid.NewGuid();

        //Act
        var fakeIdResult = async () => await Service.GetById(fakeId, default);

        //Assert
        await Should.ThrowAsync<UnknownIdentifierException>(fakeIdResult);
    }

    [Fact]
    public async Task GetById_WhenCacheHit_ReturnsCachedDataAndSkipsRepo()
    {
        //Arrange
        var cachedDto = Fixture.Create<SubscriptionDto>();
        var cacheKey = RedisKeySetter.SetCacheKey<SubscriptionDto>(cachedDto.Id);

        CacheService.CacheDataWithLock(
            cacheKey,
            Arg.Any<Func<Task<SubscriptionDto?>>>(),
            default
        ).Returns(cachedDto);

        //Act
        var result = await Service.GetUserInfoById(cachedDto.Id, default);

        //Assert
        result.ShouldBe(cachedDto);

        await SubscriptionRepository.DidNotReceive().GetUserInfoById(Arg.Any<Guid>(), default);
        await CacheService.Received(1).CacheDataWithLock(
            cacheKey,
            Arg.Any<Func<Task<SubscriptionDto?>>>(),
            default
        );
    }
    
    [Fact]
    public async Task GetAll_WhenFilteredByName_ReturnsCorrectSubscription()
    {
        //Arrange
        var subscriptionToFind = Fixture.Create<Subscription>();
        var subscriptionDto = Fixture.Build<SubscriptionDto>()
            .With(s => s.Id, subscriptionToFind.Id)
            .With(s => s.Name, subscriptionToFind.Name)
            .Create();

        var filter = new SubscriptionFilterDto { Name = subscriptionToFind.Name };
        var paginationParams = new PaginationParameters { PageNumber = 1, PageSize = 10 };
        
        var paginatedResult = new PaginatedList<Subscription>(
            new List<Subscription> { subscriptionToFind }, 
            PageNumber: 1, 
            PageSize: 10, 
            PageCount: 1, 
            TotalCount: 1
        );
        
        SubscriptionRepository.GetAll(
                Arg.Any<Expression<Func<Subscription, bool>>>(),
                Arg.Is<PaginationParameters>(p => p.PageNumber == 1),
                Arg.Any<CancellationToken>()
            )
            .Returns(paginatedResult);
        
        Mapper.Map<SubscriptionDto>(subscriptionToFind).Returns(subscriptionDto);

        //Act
        var result = await Service.GetAll(filter, paginationParams, default);

        //Assert
        await SubscriptionRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<Subscription, bool>>>(),
            Arg.Any<PaginationParameters>(),
            Arg.Any<CancellationToken>()
        );

        result.ShouldNotBeNull();
        result.Items.ShouldHaveSingleItem();
        result.Items.First().Name.ShouldBe(subscriptionToFind.Name);
        result.TotalCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentName_ReturnsEmptyPaginatedList()
    {
        //Arrange
        var filter = new SubscriptionFilterDto { Name = "LetThatSinkIn" };
        var paginationParams = new PaginationParameters { PageNumber = 1, PageSize = 10 };
        
        var emptyPaginatedResult = new PaginatedList<Subscription>(
            new List<Subscription>(), 
            PageNumber: 1, 
            PageSize: 10, 
            PageCount: 0, 
            TotalCount: 0
        );
        
        SubscriptionRepository.GetAll(
                Arg.Any<Expression<Func<Subscription, bool>>>(),
                Arg.Any<PaginationParameters>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(emptyPaginatedResult);

        //Act
        var result = await Service.GetAll(filter, paginationParams, default);

        //Assert
        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
        result.PageCount.ShouldBe(0);
        
        await SubscriptionRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<Subscription, bool>>>(),
            Arg.Any<PaginationParameters>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetAll_WhenNoSubscriptions_ReturnsEmptyPaginatedList()
    {
        //Arrange
        var filter = new SubscriptionFilterDto();
        var paginationParams = new PaginationParameters { PageNumber = 1, PageSize = 10 };
        
        var emptyPaginatedResult = new PaginatedList<Subscription>(
            new List<Subscription>(), 
            PageNumber: 1, 
            PageSize: 10, 
            PageCount: 0, 
            TotalCount: 0
        );
        
        SubscriptionRepository.GetAll(
                Arg.Any<Expression<Func<Subscription, bool>>>(),
                Arg.Any<PaginationParameters>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(emptyPaginatedResult);

        //Act
        var result = await Service.GetAll(filter, paginationParams, default);

        //Assert
        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
        result.PageCount.ShouldBe(0);
        result.PageNumber.ShouldBe(1);
    }

    [Fact]
    public async Task GetAll_WhenFilterIsEmpty_ReturnsAllPaginatedSubscriptions()
    {
        //Arrange
        var subscriptions = Fixture.CreateMany<Subscription>(3).ToList();
        var subscriptionDtos = Fixture.CreateMany<SubscriptionDto>(3).ToList();
        
        var paginatedEntities = new PaginatedList<Subscription>(
            subscriptions, 
            PageNumber: 1, 
            PageSize: 10, 
            PageCount: 1, 
            TotalCount: 3
        );

        var filter = new SubscriptionFilterDto();
        var paginationParams = new PaginationParameters { PageNumber = 1, PageSize = 10 };
        
        SubscriptionRepository.GetAll(
                Arg.Any<Expression<Func<Subscription, bool>>>(),
                Arg.Any<PaginationParameters>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(paginatedEntities);
        
        Mapper.Map<SubscriptionDto>(subscriptions[0]).Returns(subscriptionDtos[0]);
        Mapper.Map<SubscriptionDto>(subscriptions[1]).Returns(subscriptionDtos[1]);
        Mapper.Map<SubscriptionDto>(subscriptions[2]).Returns(subscriptionDtos[2]);

        //Act
        var result = await Service.GetAll(filter, paginationParams, default);

        //Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeEmpty();
        result.Items.Count.ShouldBe(3);
        result.TotalCount.ShouldBe(3);
        result.Items.ShouldBe(subscriptionDtos);
    }
    
    [Fact]
    public async Task Create_WhenValidModel_ReturnsCreatedSubscription()
    {
        //Arrange
        var auth0Id = Fixture.Create<string>();
        var createDto = Fixture.Create<CreateSubscriptionDto>();
        var existingUser = Fixture.Create<User>();

        var subscriptionEntity = Fixture.Build<Subscription>()
            .With(s => s.Name, createDto.Name)
            .With(s => s.UserId, existingUser.Id)
            .Create();

        var subscriptionDto = Fixture.Build<SubscriptionDto>()
            .With(s => s.Name, createDto.Name)
            .Create();
        
        UserRepository.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>())
            .Returns(existingUser);
        
        var emptyPagedResult = new PaginatedList<Subscription>([], 1, 10, 0, 0);
        SubscriptionRepository.GetAll(
                Arg.Any<Expression<Func<Subscription, bool>>>(), 
                Arg.Any<PaginationParameters>(),
                Arg.Any<CancellationToken>())
            .Returns(emptyPagedResult);
        
        Mapper.Map<Subscription>(createDto).Returns(subscriptionEntity);
        
        SubscriptionRepository.Create(subscriptionEntity, Arg.Any<CancellationToken>())
            .Returns(subscriptionEntity);

        Mapper.Map<SubscriptionDto>(subscriptionEntity).Returns(subscriptionDto);

        //Act
        var result = await Service.Create(auth0Id, createDto, default);

        //Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(createDto.Name);

        await UserRepository.Received(1).GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>());

        // Verify Repository Calls
        await SubscriptionRepository.Received(1).Create(
            Arg.Is<Subscription>(s => s.Name == createDto.Name && s.UserId == existingUser.Id), 
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
        var auth0Id = Fixture.Create<string>();
        var createDto = Fixture.Create<CreateSubscriptionDto>();
        var existingUser = Fixture.Create<User>();

        var existingSubscription = Fixture.Build<Subscription>()
            .With(s => s.Name, createDto.Name)
            .With(s => s.UserId, existingUser.Id)
            .With(s => s.Active, true)
            .Create();

        UserRepository.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>())
            .Returns(existingUser);
        
        SubscriptionRepository.GetByPredicate(Arg.Any<Expression<Func<Subscription, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(existingSubscription);

        //Act & Assert
        var exception = await Assert.ThrowsAsync<PolicyViolationException>(() => 
            Service.Create(auth0Id, createDto, default));

        exception.Message.ShouldContain(createDto.Name);

        await SubscriptionRepository.DidNotReceive().Create(Arg.Any<Subscription>(), Arg.Any<CancellationToken>());
        await HistoryRepository.DidNotReceive().Create(Arg.Any<Guid>(), Arg.Any<SubscriptionAction>(), Arg.Any<decimal?>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Create_WhenUserNotExists_ReturnsNull()
    {
        //Arrange
        var createDto = Fixture.Create<CreateSubscriptionDto>();

        //Act & Assert
        await Should.ThrowAsync<UnknownIdentifierException>(async () =>
            await Service.Create(string.Empty, createDto, default));
    }
}
