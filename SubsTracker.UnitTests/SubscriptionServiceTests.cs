using System.Linq.Expressions;
using AutoFixture;
using NSubstitute;
using Shouldly;
using SubsTracker.BLL.DispatchR.Handlers.UpcomingBills;
using SubsTracker.BLL.DispatchR.Signals;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
using SubsTracker.Domain.Pagination;
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

        UserRepository.GetByAuth0Id(auth0Id, ct)
            .Returns(Task.FromResult<UserEntity?>(userEntity));

        SubscriptionRepository.GetById(subscriptionId, ct)
            .Returns(Task.FromResult<SubscriptionEntity?>(subscriptionEntity));

        SubscriptionRepository.Update(Arg.Any<SubscriptionEntity>(), ct)
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
            ct
        );

        await Mediator.Received(1).Publish(
            Arg.Is<SubscriptionSignals.Canceled>(s => 
                s.Subscription.Id == subscriptionId && 
                s.UserId == userId), 
            ct);
    }
    
    [Fact]
    public async Task CancelSubscription_WhenIncorrectId_ReturnsNotFoundException()
    {
        //Arrange
        var ct = CancellationToken.None;
        var emptyDto = new UpdateSubscriptionDto();

        //Act
        var result = async () => await Service.Update(Guid.Empty, emptyDto, ct);

        //Assert
        await result.ShouldThrowAsync<UnknownIdentifierException>();
    }

    [Fact]
    public async Task CancelSubscription_WhenSuccessful_PublishesSignal()
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
    
        var updatedEntity = Fixture.Build<SubscriptionEntity>()
            .With(s => s.Id, subscriptionId)
            .With(s => s.Active, false)
            .Create();

        var expectedDto = Fixture.Build<SubscriptionDto>()
            .With(d => d.Id, subscriptionId)
            .Create();
        
        UserRepository.GetByAuth0Id(auth0Id, ct)
            .Returns(userEntity);
        SubscriptionRepository.GetById(subscriptionId, ct)
            .Returns(subscriptionEntity);
        SubscriptionRepository.Update(Arg.Any<SubscriptionEntity>(), ct)
            .Returns(updatedEntity);
        Mapper.Map<SubscriptionDto>(updatedEntity)
            .Returns(expectedDto);

        //Act
        var result = await Service.CancelSubscription(auth0Id, subscriptionId, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(subscriptionId);
        
        await SubscriptionRepository.Received(1).Update(
            Arg.Is<SubscriptionEntity>(s => s.Id == subscriptionId && s.Active == false), 
            ct);
        await Mediator.Received(1).Publish(
            Arg.Is<SubscriptionSignals.Canceled>(s => 
                s.Subscription.Id == subscriptionId && 
                s.UserId == userId), 
            ct);
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
    
        UserRepository.GetByAuth0Id(auth0Id, ct)
            .Returns(Task.FromResult<UserEntity?>(userEntity));
    
        SubscriptionRepository.GetById(subscriptionId, ct)
            .Returns(Task.FromResult<SubscriptionEntity?>(subscriptionEntity));

        SubscriptionRepository.Update(Arg.Any<SubscriptionEntity>(), ct)
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

        await UserRepository.Received(1).GetByAuth0Id(auth0Id, ct);
        await SubscriptionRepository.Received(1).Update(
            Arg.Is<SubscriptionEntity>(s => s.Id == subscriptionId), 
            ct
        );
    }

    [Fact]
    public async Task Update_WhenIncorrectId_ReturnsNotFoundException()
    {
        //Arrange
        var ct = CancellationToken.None;
        var emptyDto = new UpdateSubscriptionDto();

        //Act
        var result = async () => await Service.Update(Guid.Empty, emptyDto, ct);

        //Assert
        await result.ShouldThrowAsync<UnknownIdentifierException>();
    }
    
    [Fact]
    public async Task RenewSubscription_WhenValidModel_ReturnsRenewSubscriptionAndPublishesSignal()
    {
        //Arrange
        var ct = CancellationToken.None;
        var monthsToRenew = 2;
        var originalDueDate = new DateOnly(2025, 1, 15);
        var expectedDueDate = originalDueDate.AddMonths(monthsToRenew);
        var userId = Guid.NewGuid();

        var subscriptionEntity = Fixture.Build<SubscriptionEntity>()
            .With(s => s.DueDate, originalDueDate)
            .With(s => s.UserId, userId)
            .With(s => s.Active, false)
            .Create();

        var subscriptionDto = Fixture.Build<SubscriptionDto>()
            .With(s => s.Id, subscriptionEntity.Id)
            .With(s => s.DueDate, expectedDueDate)
            .Create();

        SubscriptionRepository.GetUserInfoById(subscriptionEntity.Id, ct)
            .Returns(subscriptionEntity);
        SubscriptionRepository.Update(Arg.Any<SubscriptionEntity>(), ct)
            .Returns(x => (SubscriptionEntity)x[0]);
        Mapper.Map<SubscriptionDto>(subscriptionEntity)
            .Returns(subscriptionDto);
        
        //Act
        var result = await Service.RenewSubscription(subscriptionEntity.Id, monthsToRenew, ct);

        //Assert
        result.ShouldNotBeNull();
        result.DueDate.ShouldBe(expectedDueDate);
        
        await SubscriptionRepository.Received(1).Update(
            Arg.Is<SubscriptionEntity>(s => 
                s.Id == subscriptionEntity.Id && 
                s.DueDate == expectedDueDate && 
                s.Active == true), 
            ct
        );
        await Mediator.Received(1).Publish(
            Arg.Is<SubscriptionSignals.Renewed>(s => 
                s.Subscription.Id == subscriptionEntity.Id && 
                s.UserId == userId), 
            ct);
    }

    [Fact]
    public async Task RenewSubscription_WhenInvalidMonthsValue_ReturnsValidationException()
    {
        //Arrange
        var monthsToRenew = -1;
        var ct = CancellationToken.None;
        var subscription = Fixture.Create<SubscriptionEntity>();

        SubscriptionRepository.GetUserInfoById(subscription.Id, ct)
            .Returns(subscription);

        //Act & Assert
        await Should.ThrowAsync<InvalidRequestDataException>(async () =>
            await Service.RenewSubscription(subscription.Id, monthsToRenew, ct));
    }
    
    [Fact]
    public async Task GetUpcomingBills_WhenCalled_DelegatesToMediator()
    {
        //Arrange
        var ct = CancellationToken.None;
        var auth0Id = "auth0|test-user";
        
        var expectedBills = Fixture.CreateMany<SubscriptionDto>(3).ToList();
        
        Mediator.Send(Arg.Is<GetUpcomingBills>(q => q.Auth0Id == auth0Id), ct)
            .Returns(expectedBills);

        //Act
        var result = await Service.GetUpcomingBills(auth0Id, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result.ShouldBe(expectedBills);
        
        await Mediator.Received(1).Send(Arg.Any<GetUpcomingBills>(), ct);
        await UserRepository.DidNotReceive().GetByAuth0Id(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await SubscriptionRepository.DidNotReceive().GetUpcomingBills(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetUpcomingBills_WhenNoBillsAreDue_ReturnsEmptyCollectionFromMediator()
    {
        //Arrange
        var ct = CancellationToken.None;
        var auth0Id = "auth0|test-user";
        
        List<SubscriptionDto> emptyList = [];
        
        Mediator.Send(Arg.Is<GetUpcomingBills>(q => q.Auth0Id == auth0Id), ct)
            .Returns(emptyList);

        //Act
        var result = await Service.GetUpcomingBills(auth0Id, ct);

        //Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
        
        await Mediator.Received(1).Send(Arg.Is<GetUpcomingBills>(q => q.Auth0Id == auth0Id), ct);
        await UserRepository.DidNotReceive().GetByAuth0Id(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await SubscriptionRepository.DidNotReceive().GetUpcomingBills(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
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

        var cacheKey = $"{subscriptionEntity.Id}:{nameof(SubscriptionEntity)}";

        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<Func<Task<SubscriptionDto?>>>(),
            ct
        ).Returns(async callInfo =>
        {
            var factory = callInfo.Arg<Func<Task<SubscriptionDto?>>>();
            return await factory();
        });

        SubscriptionRepository.GetUserInfoById(subscriptionEntity.Id, ct)
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

        await SubscriptionRepository.Received(1).GetUserInfoById(subscriptionEntity.Id, ct);
        await CacheService.Received(1).CacheDataWithLock(
            Arg.Is<string>(s => s == cacheKey),
            Arg.Any<Func<Task<SubscriptionDto?>>>(),
            ct
        );
    }

    [Fact]
    public async Task GetById_WhenEmptyGuid_ThrowsUnknownIdentifierException()
    {
        //Arrange
        var emptyId = Guid.Empty;

        //Act
        async Task Act() => await Service.GetById(emptyId, CancellationToken.None);

        //Assert
        await Should.ThrowAsync<UnknownIdentifierException>(Act);
    }

    [Fact]
    public async Task GetById_WhenSubscriptionDoesNotExist_ThrowsNotFoundException()
    {
        //Arrange
        var fakeId = Guid.NewGuid();
        var ct = CancellationToken.None;

        //Act
        async Task Act() => await Service.GetById(fakeId, ct);

        //Assert
        await Should.ThrowAsync<UnknownIdentifierException>(Act);
    }

    [Fact]
    public async Task GetById_WhenCacheHit_ReturnsCachedDataAndSkipsRepo()
    {
        //Arrange
        var ct = CancellationToken.None;
        var cachedDto = Fixture.Create<SubscriptionDto>();
        
        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<Func<Task<SubscriptionDto?>>>(),
            ct
        ).Returns(Task.FromResult<SubscriptionDto?>(cachedDto));

        //Act
        var result = await Service.GetById(cachedDto.Id, ct);

        //Assert
        result.ShouldNotBeNull();
        result.ShouldBe(cachedDto);

        await SubscriptionRepository.DidNotReceive().GetById(Arg.Any<Guid>(), ct);
        await CacheService.Received(1).CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<Func<Task<SubscriptionDto?>>>(),
            ct
        );
    }
    
    [Fact]
    public async Task GetAll_WhenFilteredByName_ReturnsCorrectSubscription()
    {
        //Arrange
        var ct = CancellationToken.None;
        var subscriptionToFind = Fixture.Create<SubscriptionEntity>();
        var subscriptionDto = Fixture.Build<SubscriptionDto>()
            .With(s => s.Id, subscriptionToFind.Id)
            .With(s => s.Name, subscriptionToFind.Name)
            .Create();

        var filter = new SubscriptionFilterDto { Name = subscriptionToFind.Name };
    
        var pagedList = new PaginatedList<SubscriptionEntity>([subscriptionToFind], 1, 10, 1);
    
        SubscriptionRepository.GetAll(
                Arg.Any<Expression<Func<SubscriptionEntity, bool>>>(), 
                Arg.Any<PaginationParameters?>(), 
                Arg.Is(ct))
            .Returns(pagedList);
        
        Mapper.Map<SubscriptionDto>(Arg.Is<SubscriptionEntity>(s => s.Id == subscriptionToFind.Id))
            .Returns(subscriptionDto);

        //Act
        var result = await Service.GetAll(filter, null, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Items.ShouldHaveSingleItem();
        result.Items[0].Name.ShouldBe(subscriptionToFind.Name);

        await SubscriptionRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<SubscriptionEntity, bool>>>(),
            Arg.Any<PaginationParameters?>(),
            ct
        );
    }

    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentName_ReturnsEmptyList()
    {
        //Arrange
        var ct = CancellationToken.None;
        var filter = new SubscriptionFilterDto { Name = "NonExistentName123" };
        
        var emptyPagedList = new PaginatedList<SubscriptionEntity>([], 1, 10, 0);
        
        SubscriptionRepository.GetAll(
                Arg.Any<Expression<Func<SubscriptionEntity, bool>>>(), 
                Arg.Any<PaginationParameters?>(), 
                Arg.Is(ct))
            .Returns(emptyPagedList);
        
        Mapper.Map<List<SubscriptionDto>>(Arg.Any<List<SubscriptionEntity>>()).Returns([]);

        //Act
        var result = await Service.GetAll(filter, null, ct);

        //Assert
        result.Items.ShouldBeEmpty();
    
        await SubscriptionRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<SubscriptionEntity, bool>>>(),
            Arg.Any<PaginationParameters?>(),
            ct
        );
    }

    [Fact]
    public async Task GetAll_WhenNoSubscriptions_ReturnsEmptyList()
    {
        //Arrange
        var ct = CancellationToken.None;
        var filter = new SubscriptionFilterDto();
        
        var emptyPagedList = new PaginatedList<SubscriptionEntity>([], 1, 10, 0);
        
        SubscriptionRepository.GetAll(
                Arg.Any<Expression<Func<SubscriptionEntity, bool>>>(), 
                Arg.Any<PaginationParameters?>(), 
                Arg.Is(ct))
            .Returns(emptyPagedList);
        
        Mapper.Map<List<SubscriptionDto>>(Arg.Any<List<SubscriptionEntity>>()).Returns([]);

        //Act
        var result = await Service.GetAll(filter, null, ct);

        //Assert
        result.Items.ShouldBeEmpty();
    
        await SubscriptionRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<SubscriptionEntity, bool>>>(),
            Arg.Any<PaginationParameters?>(),
            ct
        );
    }

    [Fact]
    public async Task GetAll_WhenFilterIsEmpty_ReturnsAllSubscriptions()
    {
        //Arrange
        var ct = CancellationToken.None;
        var filter = new SubscriptionFilterDto();

        List<SubscriptionEntity> subscriptions = [.. Fixture.CreateMany<SubscriptionEntity>(3)];
        List<SubscriptionDto> subscriptionDtos = [.. Fixture.CreateMany<SubscriptionDto>(3)];
        
        var pagedList = new PaginatedList<SubscriptionEntity>(subscriptions, 1, 10, 3);
    
        SubscriptionRepository.GetAll(
                Arg.Any<Expression<Func<SubscriptionEntity, bool>>>(), 
                Arg.Any<PaginationParameters?>(), 
                Arg.Is(ct))
            .Returns(pagedList);
        
        Mapper.Map<SubscriptionDto>(Arg.Any<SubscriptionEntity>())
            .Returns(subscriptionDtos[0], subscriptionDtos[1], subscriptionDtos[2]);
        
        //Act
        var result = await Service.GetAll(filter, null, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(3);
        result.Items.ShouldBe(subscriptionDtos);

        await SubscriptionRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<SubscriptionEntity, bool>>>(),
            Arg.Any<PaginationParameters?>(),
            ct
        );
    }
    
    [Theory]
    [InlineData(SubscriptionType.Lifetime, SubscriptionContent.Design)]
    [InlineData(SubscriptionType.Free, SubscriptionContent.News)]
    public async Task GetAll_WhenFilteredByEnums_ReturnsMatchingSubscriptions(SubscriptionType type, SubscriptionContent content)
    {
        //Arrange
        var ct = CancellationToken.None;
        var filter = new SubscriptionFilterDto { Type = type, Content = content };

        List<SubscriptionEntity> entities = [.. Fixture.Build<SubscriptionEntity>()
            .With(s => s.Type, type)
            .With(s => s.Content, content)
            .CreateMany(1)];

        List<SubscriptionDto> dtos = [.. Fixture.Build<SubscriptionDto>()
            .With(d => d.Type, type)
            .With(d => d.Content, content)
            .CreateMany(1)];
    
        var pagedList = new PaginatedList<SubscriptionEntity>(entities, 1, 10, 1);
    
        SubscriptionRepository.GetAll(
                Arg.Any<Expression<Func<SubscriptionEntity, bool>>>(), 
                Arg.Any<PaginationParameters?>(), 
                Arg.Is(ct))
            .Returns(pagedList);
        
        Mapper.Map<SubscriptionDto>(Arg.Is<SubscriptionEntity>(s => s.Id == entities[0].Id))
            .Returns(dtos[0]);

        //Act
        var result = await Service.GetAll(filter, null, ct);

        //Assert
        result.Items.ShouldHaveSingleItem();
        result.Items[0].Type.ShouldBe(type);
        result.Items[0].Content.ShouldBe(content);

        await SubscriptionRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<SubscriptionEntity, bool>>>(),
            Arg.Any<PaginationParameters?>(),
            ct
        );
    }
    
    [Fact]
    public async Task GetAll_WhenFilteredByUserId_ReturnsUserSubscriptions()
    {
        //Arrange
        var ct = CancellationToken.None;
        var userId = Guid.NewGuid();
        var filter = new SubscriptionFilterDto { UserId = userId };
    
        List<SubscriptionEntity> userSubscriptions = [.. Fixture.Build<SubscriptionEntity>()
            .With(s => s.UserId, userId)
            .CreateMany(2)];
    
        List<SubscriptionDto> subscriptionDtos = [.. Fixture.CreateMany<SubscriptionDto>(2)];
        
        var pagedList = new PaginatedList<SubscriptionEntity>(userSubscriptions, 1, 10, 2);
        
        SubscriptionRepository.GetAll(
                Arg.Any<Expression<Func<SubscriptionEntity, bool>>>(), 
                Arg.Any<PaginationParameters?>(), 
                Arg.Is(ct))
            .Returns(pagedList);

        Mapper.Map<List<SubscriptionDto>>(userSubscriptions).Returns(subscriptionDtos);

        //Act
        var result = await Service.GetAll(filter, null, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(2);
    
        await SubscriptionRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<SubscriptionEntity, bool>>>(),
            Arg.Any<PaginationParameters?>(),
            ct
        );
    }
    
    [Fact]
    public async Task Create_WhenValidModel_ReturnsCreatedSubscriptionAndPublishesSignal()
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
        
        UserRepository.GetByAuth0Id(auth0Id, ct).Returns(existingUser);
        SubscriptionRepository.GetByPredicate(Arg.Any<Expression<Func<SubscriptionEntity, bool>>>(), ct)
            .Returns(Task.FromResult<SubscriptionEntity?>(null));
        Mapper.Map<SubscriptionEntity>(createDto)
            .Returns(subscriptionEntity);
        SubscriptionRepository.Create(subscriptionEntity, ct)
            .Returns(subscriptionEntity);
        Mapper.Map<SubscriptionDto>(subscriptionEntity)
            .Returns(subscriptionDto);

        //Act
        var result = await Service.Create(auth0Id, createDto, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(createDto.Name);
        
        await UserRepository.Received(1).GetByAuth0Id(auth0Id, ct);
        await SubscriptionRepository.Received(1).Create(
            Arg.Is<SubscriptionEntity>(s => s.Name == createDto.Name && s.UserId == existingUser.Id), 
            ct);
        await Mediator.Received(1).Publish(
            Arg.Is<SubscriptionSignals.Created>(s => 
                s.Subscription.Id == subscriptionEntity.Id && 
                s.UserId == existingUser.Id), 
            ct);
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

        UserRepository.GetByAuth0Id(auth0Id, ct)
            .Returns(existingUser);
        SubscriptionRepository.GetByPredicate(Arg.Any<Expression<Func<SubscriptionEntity, bool>>>(), ct)
            .Returns(existingSubscription);

        //Act
        var act = async () => await Service.Create(auth0Id, createDto, ct);

        //Assert
        var exception = await act.ShouldThrowAsync<PolicyViolationException>();
        exception.Message.ShouldContain(createDto.Name);
        
        await SubscriptionRepository.DidNotReceive().Create(Arg.Any<SubscriptionEntity>(), ct);
        await Mediator.DidNotReceive().Publish(Arg.Any<SubscriptionSignals.Created>(), ct);
    }
    
    [Fact]
    public async Task Create_WhenUserNotExists_ReturnsNull()
    {
        //Arrange
        var ct = CancellationToken.None;
        var createDto = Fixture.Create<CreateSubscriptionDto>();

        //Act & Assert
        await Should.ThrowAsync<UnknownIdentifierException>(async () =>
            await Service.Create(string.Empty, createDto, ct));
    }
}
