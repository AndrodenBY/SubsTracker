namespace SubsTracker.UnitTests.SubscriptionService;

public class SubscriptionServiceCancelSubscriptionTests : SubscriptionServiceTestsBase
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
}
