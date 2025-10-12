namespace SubsTracker.UnitTests.SubscriptionService;

public class SubscriptionServiceGetUpcomingBillsTests : SubscriptionServiceTestsBase
{
    [Fact]
    public async Task GetUpcomingBills_WhenMultipleSubscriptionsAreDue_ReturnsAllUpcomingBills()
    {
        //Arrange
        var user = Fixture.Create<User>();

        var dueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1));

        var subscriptions = Fixture.Build<Subscription>()
            .With(s => s.UserId, user.Id)
            .With(s => s.User, user)
            .With(s => s.DueDate, dueDate)
            .CreateMany(3)
            .ToList();

        var subscriptionDtos = subscriptions.Select(s => Fixture.Build<SubscriptionDto>()
            .With(dto => dto.Id, s.Id)
            .With(dto => dto.Name, s.Name)
            .With(dto => dto.DueDate, s.DueDate)
            .Create()).ToList();

        Repository.GetUpcomingBills(user.Id, default)
           .Returns(subscriptions);

        Mapper.Map<List<SubscriptionDto>>(subscriptions)
           .Returns(subscriptionDtos);

        //Act
        var result = await Service.GetUpcomingBills(user.Id, default);

        //Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
    }

    [Fact]
    public async Task GetUpcomingBills_WhenSubscriptionsExistButNoneAreDueSoon_ReturnsEmptyCollection()
    {
        //Arrange
        var user = Fixture.Create<User>();

        var futureDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));

        var subscriptions = Fixture.Build<Subscription>()
            .With(s => s.UserId, user.Id)
            .With(s => s.User, user)
            .With(s => s.DueDate, futureDueDate)
            .CreateMany(3)
            .ToList();

        Repository.GetUpcomingBills(user.Id, default)
           .Returns(subscriptions);
        Mapper.Map<List<SubscriptionDto>>(subscriptions)
           .Returns(new List<SubscriptionDto>());

        //Act
        var result = await Service.GetUpcomingBills(user.Id, default);

        //Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }
}
