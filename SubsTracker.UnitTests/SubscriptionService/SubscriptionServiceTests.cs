using SubsTracker.BLL.Handlers.Signals.Subscription;
using SubsTracker.BLL.Handlers.UpcomingBills;
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
        var ct = CancellationToken.None;

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

        UserRepository.GetByAuth0Id(auth0Id, ct)
            .Returns(userEntity);

        SubscriptionRepository.GetById(subscriptionId, ct)
            .Returns(subscriptionEntity);

        SubscriptionRepository.Update(Arg.Any<Subscription>(), ct)
            .Returns(updatedSubscriptionEntity);

        Mapper.Map<SubscriptionDto>(updatedSubscriptionEntity)
            .Returns(updatedSubscriptionDto);

        //Act
        var result = await Service.CancelSubscription(auth0Id, subscriptionId, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(subscriptionId);

        await SubscriptionRepository.Received(1)
            .Update(Arg.Is<Subscription>(s => s.Id == subscriptionId && !s.Active), ct);

        await Mediator.Received(1).Publish(
            Arg.Is<SubscriptionCanceledSignal>(s => 
                s.Subscription.Id == subscriptionId && 
                s.UserId == userId), 
            ct);
    }

    [Fact]
    public async Task CancelSubscription_WhenIncorrectId_ReturnsNotFoundException()
    {
        //Arrange
        var emptyDto = new UpdateSubscriptionDto();

        //Act
        var result = async () => await Service.Update(Guid.Empty, emptyDto, CancellationToken.None);

        //Assert
        await result.ShouldThrowAsync<UnknownIdentifierException>();
    }

    [Fact]
    public async Task CancelSubscription_WhenSuccessful_DeletesAndPublishesSignal()
    {
        //Arrange
        var auth0Id = "auth0|123456789";
        var userId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var ct = CancellationToken.None;
    
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
    
        UserRepository.GetByAuth0Id(auth0Id, ct).Returns(userEntity);
    
        SubscriptionRepository.GetById(subscriptionId, ct).Returns(subscriptionEntity);
    
        SubscriptionRepository.Update(Arg.Any<Subscription>(), ct).Returns(cancelledEntity);
    
        Mapper.Map<SubscriptionDto>(cancelledEntity).Returns(Fixture.Create<SubscriptionDto>());

        //Act
        await Service.CancelSubscription(auth0Id, subscriptionId, ct);

        //Assert
        await SubscriptionRepository.Received(1)
            .Update(Arg.Is<Subscription>(s => s.Id == subscriptionId && !s.Active), ct);

        await Mediator.Received(1).Publish(
            Arg.Is<SubscriptionCanceledSignal>(s => 
                s.Subscription.Id == subscriptionId && 
                s.UserId == userId), 
            ct);
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
        var result = await Service.Update(auth0Id, updateDto, CancellationToken.None);

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
        var result = async () => await Service.Update(Guid.Empty, emptyDto, CancellationToken.None);

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

        SubscriptionRepository.GetUserInfoById(Arg.Any<Guid>(), CancellationToken.None).Returns(subscriptionEntity);
        SubscriptionRepository.Update(Arg.Any<Subscription>(), CancellationToken.None).Returns(subscriptionEntity);
        Mapper.Map<SubscriptionDto>(subscriptionEntity).Returns(subscriptionDto);

        //Act
        var result = await Service.RenewSubscription(subscriptionEntity.Id, monthsToRenew, CancellationToken.None);

        //Assert
        result.ShouldNotBeNull();
        result.DueDate.ShouldBe(originalDueDate.AddMonths(monthsToRenew));
        await SubscriptionRepository.Received(1)
            .Update(Arg.Is<Subscription>(s => s.DueDate == originalDueDate.AddMonths(monthsToRenew)), CancellationToken.None);
    }

    [Fact]
    public async Task RenewSubscription_WhenInvalidMonthsValue_ReturnsValidationException()
    {
        //Arrange
        var monthsToRenew = -1;
        var subscription = Fixture.Create<Subscription>();

        SubscriptionRepository.GetUserInfoById(subscription.Id, CancellationToken.None)
            .Returns(subscription);

        //Act & Assert
        await Should.ThrowAsync<InvalidRequestDataException>(async () =>
            await Service.RenewSubscription(subscription.Id, monthsToRenew, CancellationToken.None));
    }
    
    [Fact]
    public async Task GetUpcomingBills_WhenMultipleSubscriptionsAreDue_ReturnsAllUpcomingBills()
    {
        //Arrange
        var auth0Id = Fixture.Create<string>();
        var ct = CancellationToken.None;
        var expectedDtos = Fixture.Create<List<SubscriptionDto>>();
        
        Mediator.Send(Arg.Is<GetUpcomingBills>(q => q.Auth0Id == auth0Id), ct)
            .Returns(new ValueTask<List<SubscriptionDto>>(expectedDtos));

        //Act
        var result = await Service.GetUpcomingBills(auth0Id, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result.ShouldBe(expectedDtos);
        await Mediator.Received(1).Send(Arg.Is<GetUpcomingBills>(q => q.Auth0Id == auth0Id), ct).AsTask();
        await UserRepository.DidNotReceiveWithAnyArgs().GetByAuth0Id(Arg.Any<string>(), ct);
        await SubscriptionRepository.DidNotReceiveWithAnyArgs().GetUpcomingBills(Arg.Any<Guid>(), ct);
        await CacheService.DidNotReceiveWithAnyArgs().RemoveData(Arg.Any<List<string>>(), ct);
    }

    [Fact]
    public async Task GetUpcomingBills_WhenCalled_ReturnsExpectedCollection()
    {
        //Arrange
        var auth0Id = Fixture.Create<string>();
        var ct = CancellationToken.None;
        var expectedBills = Fixture.Create<List<SubscriptionDto>>();
        
        Mediator.Send(Arg.Is<GetUpcomingBills>(q => q.Auth0Id == auth0Id), ct)
            .Returns(expectedBills);

        //Act
        var result = await Service.GetUpcomingBills(auth0Id, ct);

        //Assert
        result.ShouldNotBeNull();
        result.ShouldBe(expectedBills);
        
        await Mediator.Received(1).Send(Arg.Is<GetUpcomingBills>(q => q.Auth0Id == auth0Id), ct);
        
        await UserRepository.DidNotReceiveWithAnyArgs().GetByAuth0Id(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await SubscriptionRepository.DidNotReceiveWithAnyArgs().GetUpcomingBills(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
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

        var cacheKey = $"{subscriptionDto.Id}:{nameof(Subscription)}";

        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<Func<Task<SubscriptionDto?>>>(),
            CancellationToken.None
        )!.Returns(callInfo =>
        {
            var factory = callInfo.Arg<Func<Task<SubscriptionDto>>>();
            return factory();
        });
        SubscriptionRepository.GetUserInfoById(subscriptionEntity.Id, CancellationToken.None)
            .Returns(subscriptionEntity);

        Mapper.Map<SubscriptionDto>(subscriptionEntity)
            .Returns(subscriptionDto);

        //Act
        var result = await Service.GetUserInfoById(subscriptionEntity.Id, CancellationToken.None);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(subscriptionEntity.Id);
        result.Name.ShouldBe(subscriptionEntity.Name);
        result.Price.ShouldBe(subscriptionEntity.Price);
        result.DueDate.ShouldBe(subscriptionEntity.DueDate);

        await SubscriptionRepository.Received(1).GetUserInfoById(subscriptionEntity.Id, CancellationToken.None);
        await CacheService.Received(1).CacheDataWithLock(
            cacheKey,
            Arg.Any<Func<Task<SubscriptionDto?>>>(),
            CancellationToken.None
        );
    }

    [Fact]
    public async Task GetById_WhenEmptyGuid_ThrowsNotFoundException()
    {
        //Arrange
        var emptyId = Guid.Empty;

        //Act
        var emptyIdResult = async() => await Service.GetById(emptyId, CancellationToken.None);

        //Assert
        await Should.ThrowAsync<UnknownIdentifierException>(emptyIdResult);
    }

    [Fact]
    public async Task GetById_WhenSubscriptionDoesNotExist_ThrowsNotFoundException()
    {
        //Arrange
        var fakeId = Guid.NewGuid();

        //Act
        var fakeIdResult = async () => await Service.GetById(fakeId, CancellationToken.None);

        //Assert
        await Should.ThrowAsync<UnknownIdentifierException>(fakeIdResult);
    }

    [Fact]
    public async Task GetById_WhenCacheHit_ReturnsCachedDataAndSkipsRepo()
    {
        //Arrange
        var cachedDto = Fixture.Create<SubscriptionDto>();
        var cacheKey = RedisKeySetter.SetCacheKey<Subscription>(cachedDto.Id);

        CacheService.CacheDataWithLock(
            cacheKey,
            Arg.Any<Func<Task<SubscriptionDto?>>>(),
            CancellationToken.None
        ).Returns(cachedDto);

        //Act
        var result = await Service.GetUserInfoById(cachedDto.Id, CancellationToken.None);

        //Assert
        result.ShouldBe(cachedDto);

        await SubscriptionRepository.DidNotReceive().GetUserInfoById(Arg.Any<Guid>(), CancellationToken.None);
        await CacheService.Received(1).CacheDataWithLock(
            cacheKey,
            Arg.Any<Func<Task<SubscriptionDto?>>>(),
            CancellationToken.None
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
            [subscriptionToFind], 
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
        var result = await Service.GetAll(filter, paginationParams, CancellationToken.None);

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
        var result = await Service.GetAll(filter, paginationParams, CancellationToken.None);

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
        var result = await Service.GetAll(filter, paginationParams, CancellationToken.None);

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
        var result = await Service.GetAll(filter, paginationParams, CancellationToken.None);

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
        var ct = CancellationToken.None;

        var subscriptionEntity = Fixture.Build<Subscription>()
            .With(s => s.Name, createDto.Name)
            .With(s => s.UserId, existingUser.Id)
            .Create();

        var subscriptionDto = Fixture.Build<SubscriptionDto>()
            .With(s => s.Name, createDto.Name)
            .Create();
    
        UserRepository.GetByAuth0Id(auth0Id, ct)
            .Returns(existingUser);
    
        SubscriptionRepository.GetAll(
                Arg.Any<Expression<Func<Subscription, bool>>>(), 
                Arg.Any<PaginationParameters>(),
                ct)
            .Returns(new PaginatedList<Subscription>([], 1, 10, 0, 0));
    
        Mapper.Map<Subscription>(createDto).Returns(subscriptionEntity);
    
        SubscriptionRepository.Create(subscriptionEntity, ct)
            .Returns(subscriptionEntity);

        Mapper.Map<SubscriptionDto>(subscriptionEntity).Returns(subscriptionDto);

        //Act
        var result = await Service.Create(auth0Id, createDto, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(createDto.Name);

        await UserRepository.Received(1).GetByAuth0Id(auth0Id, ct);

        await SubscriptionRepository.Received(1).Create(
            Arg.Is<Subscription>(s => s.Name == createDto.Name && s.UserId == existingUser.Id), 
            ct);

        await Mediator.Received(1).Publish(
            Arg.Is<SubscriptionCreatedSignal>(s => 
                s.Subscription.Id == subscriptionEntity.Id && 
                s.UserId == existingUser.Id), 
            ct);
    }
    
    [Fact]
    public async Task Create_WhenSubscriptionAlreadyExists_ThrowsPolicyViolationException()
    {
        //Arrange
        var auth0Id = Fixture.Create<string>();
        var createDto = Fixture.Create<CreateSubscriptionDto>();
        var existingUser = Fixture.Create<User>();
        var ct = CancellationToken.None;
        
        var existingSubscription = Fixture.Build<Subscription>()
            .With(s => s.Name, createDto.Name)
            .With(s => s.UserId, existingUser.Id)
            .With(s => s.Active, true)
            .Create();
        
        var mappedSubscription = Fixture.Create<Subscription>();

        UserRepository.GetByAuth0Id(auth0Id, ct)
            .Returns(existingUser);
        
        SubscriptionRepository.GetByPredicate(
                Arg.Any<Expression<Func<Subscription, bool>>>(), 
                ct)
            .Returns(existingSubscription);
        
        Mapper.Map<Subscription>(createDto).Returns(mappedSubscription);

        //Act & Assert
        await Assert.ThrowsAsync<PolicyViolationException>(() => 
            Service.Create(auth0Id, createDto, ct));

        await SubscriptionRepository.DidNotReceive().Create(Arg.Any<Subscription>(), ct);
    }
    
    [Fact]
    public async Task Create_WhenUserNotExists_ReturnsNull()
    {
        //Arrange
        var createDto = Fixture.Create<CreateSubscriptionDto>();

        //Act & Assert
        await Should.ThrowAsync<UnknownIdentifierException>(async () =>
            await Service.Create(string.Empty, createDto, CancellationToken.None));
    }
}
