namespace SubsTracker.UnitTests.SubscriptionService;

public class SubscriptionServiceRenewSubscriptionTests : SubscriptionServiceTestsBase
{
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
        await SubscriptionRepository.Received(1).Update(Arg.Is<Subscription>(s => s.DueDate == originalDueDate.AddMonths(monthsToRenew)), default);
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
        await Should.ThrowAsync<ValidationException>(async () => await Service.RenewSubscription(subscription.Id, monthsToRenew, default));
    }
}
