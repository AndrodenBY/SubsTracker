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
        
        var cacheKey = $"{user.Id}_upcoming_bills";

        CacheService.GetData<List<SubscriptionDto>>(cacheKey, default)
            .Returns((List<SubscriptionDto>)null!);
        SubscriptionRepository.GetUpcomingBills(user.Id, default)
            .Returns(subscriptions);
        Mapper.Map<List<SubscriptionDto>>(subscriptions)
            .Returns(subscriptionDtos);

        //Act
        var result = await Service.GetUpcomingBills(user.Id, default);

        //Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        
        await SubscriptionRepository.Received(1).GetUpcomingBills(user.Id, default);
        await CacheService.Received(1).GetData<List<SubscriptionDto>>(cacheKey, default);
        await CacheService.Received(1).SetData(
            Arg.Is<string>(key => key == cacheKey), 
            Arg.Is<List<SubscriptionDto>>(list => list.Count == 3 && list.First().Id == subscriptionDtos.First().Id), 
            Arg.Is<TimeSpan>(ts => ts == TimeSpan.FromMinutes(3)),
            default
        );
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

        SubscriptionRepository.GetUpcomingBills(user.Id, default)
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
