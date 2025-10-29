namespace SubsTracker.UnitTests.SubscriptionService;

public class SubscriptionServiceCancelSubscriptionTests : SubscriptionServiceTestsBase
{
    [Fact]
    public async Task CancelSubscription_WhenValidModel_ReturnsInactiveSubscription()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();

        var userEntity = Fixture.Create<User>();
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

        UserRepository.GetById(userId, default)
           .Returns(userEntity);
        SubscriptionRepository.GetUserInfoById(subscriptionId, default)
           .Returns(subscriptionEntity);
        SubscriptionRepository.Update(Arg.Any<Subscription>(), default)
           .Returns(updatedSubscriptionEntity);
        Mapper.Map<SubscriptionDto>(updatedSubscriptionEntity)
           .Returns(updatedSubscriptionDto);

        //Act
        var result = await Service.CancelSubscription(userId, subscriptionId, default);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(subscriptionId);
        await SubscriptionRepository.Received(1).Update(Arg.Is<Subscription>(s => s.Id == subscriptionId && s.Active == false), default);
        await MessageService.Received(1).NotifySubscriptionCanceled(Arg.Is<SubscriptionCanceledEvent>(subscriptionCanceledEvent => subscriptionCanceledEvent.Id == subscriptionId), default);
    }

    [Fact]
    public async Task CancelSubscription_WhenIncorrectId_ReturnsNotFoundException()
    {
        //Arrange
        var emptyDto = new UpdateSubscriptionDto();

        //Act
        var result = async() => await Service.Update(Guid.Empty, emptyDto, default);
        
        //Assert
        await result.ShouldThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task CancelSubscription_WhenSuccessful_InvalidatesSubscriptionAndBillsCache()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        
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
        
        SubscriptionRepository.GetUserInfoById(subscriptionId, default).Returns(subscriptionEntity);
        SubscriptionRepository.Update(subscriptionEntity, default).Returns(cancelledEntity);
        Mapper.Map<SubscriptionDto>(cancelledEntity).Returns(Fixture.Create<SubscriptionDto>());
        
        var subscriptionCacheKey = RedisKeySetter.SetCacheKey<SubscriptionDto>(subscriptionId);
        var billsCacheKey = RedisKeySetter.SetCacheKey(userId, "upcoming_bills");

        //Act
        await Service.CancelSubscription(userId, subscriptionId, default);

        //Assert
        await CacheAccessService.Received(1).RemoveData(
            Arg.Is<List<string>>(list => 
                list.Contains(subscriptionCacheKey) && 
                list.Contains(billsCacheKey) &&
                list.Count == 2
            ), 
            default);
        
        await HistoryRepository.Received(1).Create(Arg.Any<Guid>(), SubscriptionAction.Cancel, Arg.Any<decimal?>(), default);
        await MessageService.Received(1).NotifySubscriptionCanceled(Arg.Any<SubscriptionCanceledEvent>(), default);
    }
}
