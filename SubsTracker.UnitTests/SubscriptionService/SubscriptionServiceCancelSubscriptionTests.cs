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
        Repository.GetById(subscriptionId, default)
           .Returns(subscriptionEntity);
        Repository.Update(Arg.Any<Subscription>(), default)
           .Returns(updatedSubscriptionEntity);
        Mapper.Map<SubscriptionDto>(updatedSubscriptionEntity)
           .Returns(updatedSubscriptionDto);

        //Act
        var result = await Service.CancelSubscription(userId, subscriptionId, default);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(subscriptionId);
        await Repository.Received(1).Update(Arg.Is<Subscription>(s => s.Id == subscriptionId && s.Active == false), default);
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
}
