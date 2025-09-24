namespace SubsTracker.UnitTests.SubscriptionService;

public class SubscriptionServiceRenewSubscriptionTests : SubscriptionServiceTestsBase
{
    [Fact]
    public async Task RenewSubscription_WhenValidModel_ReturnsRenewSubscription()
    {
        //Arrange
        var monthsToRenew = 2;
        var originalDueDate = new DateOnly(2025, 1, 15);
        var subscriptionEntity = _fixture.Build<Subscription>()
            .With(subscription => subscription.DueDate, originalDueDate)
            .Create();
        var subscriptionDto = _fixture.Build<SubscriptionDto>()
            .With(s => s.Id, subscriptionEntity.Id)
            .With(s => s.Name, subscriptionEntity.Name)
            .With(s => s.Price, subscriptionEntity.Price)
            .With(s => s.DueDate, originalDueDate.AddMonths(monthsToRenew))
            .With(s => s.Content, subscriptionEntity.Content)
            .With(s => s.Type, subscriptionEntity.Type)
            .Create();
        
        _repository.GetById(Arg.Any<Guid>(), default).Returns(subscriptionEntity);
        _repository.Update(Arg.Any<Subscription>(), default).Returns(subscriptionEntity);
        _mapper.Map<SubscriptionDto>(subscriptionEntity).Returns(subscriptionDto);
        
        //Act
        var result = await _service.RenewSubscription(subscriptionEntity.Id, monthsToRenew, default);
        
        //Assert
        result.ShouldNotBeNull();
        result.DueDate.ShouldBe(originalDueDate.AddMonths(monthsToRenew));
        await _repository.Received(1).Update(Arg.Is<Subscription>(s => s.DueDate == originalDueDate.AddMonths(monthsToRenew)), default);
    }

    [Fact]
    public async Task RenewSubscription_WhenInvalidMonthsValue_ReturnsValidationException()
    {
        //Arrange
        var monthsToRenew = -1;
        var subscription = _fixture.Create<Subscription>();

        _repository.GetById(subscription.Id, default)
            .Returns(subscription);

        //Act & Assert
        await Should.ThrowAsync<ValidationException>(async () => await _service.RenewSubscription(subscription.Id, monthsToRenew, default));
    }
}
