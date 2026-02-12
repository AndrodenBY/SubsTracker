namespace SubsTracker.UnitTests.SubscriptionService;

public class SubscriptionServiceGetUpcomingBillsTests : SubscriptionServiceTestsBase
{
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
}
