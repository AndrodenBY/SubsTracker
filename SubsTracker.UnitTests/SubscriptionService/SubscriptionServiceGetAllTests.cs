namespace SubsTracker.UnitTests.SubscriptionService;

public class SubscriptionServiceGetAllTests : SubscriptionServiceTestsBase
{
    [Fact]
    public async Task GetAll_WhenFilteredByName_ReturnsCorrectSubscription()
    {
        //Arrange
        var subscriptionToFind = Fixture.Create<Subscription>();
        var subscriptionDto = Fixture.Build<SubscriptionDto>()
            .With(subscription => subscription.Id, subscriptionToFind.Id)
            .With(subscription => subscription.Name, subscriptionToFind.Name)
            .Create();

        var filter = new SubscriptionFilterDto { Name = subscriptionToFind.Name };

        Repository.GetAll(Arg.Any<Expression<Func<Subscription, bool>>>(), default)
           .Returns(new List<Subscription> { subscriptionToFind });
        Mapper.Map<List<SubscriptionDto>>(Arg.Any<List<Subscription>>()).Returns(new List<SubscriptionDto> { subscriptionDto });

        //Act
        var result = await Service.GetAll(filter, default);

        //Assert
        result.ShouldNotBeNull();
        result.ShouldHaveSingleItem();
        result.Single().Name.ShouldBe(subscriptionToFind.Name);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentName_ReturnsEmptyList()
    {
        //Arrange
        var subscriptionToFind = Fixture.Create<Subscription>();
        Fixture.Build<SubscriptionDto>()
            .With(subscription => subscription.Id, subscriptionToFind.Id)
            .With(subscription => subscription.Name, subscriptionToFind.Name)
            .Create();

        var filter = new SubscriptionFilterDto { Name = "LetThatSinkIn" };

        Repository.GetAll(Arg.Any<Expression<Func<Subscription, bool>>>(), default)
           .Returns(new List<Subscription>());
        Mapper.Map<List<SubscriptionDto>>(Arg.Any<List<Subscription>>()).Returns(new List<SubscriptionDto>());

        //Act
        var result = await Service.GetAll(filter, default);

        //Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WhenNoSubscriptions_ReturnsEmptyList()
    {
        //Arrange
        var filter = new SubscriptionFilterDto();

        Repository.GetAll(Arg.Any<Expression<Func<Subscription, bool>>>(), default)
           .Returns(new List<Subscription>());
        Mapper.Map<List<SubscriptionDto>>(Arg.Any<List<Subscription>>()).Returns(new List<SubscriptionDto>());

        //Act
        var result = await Service.GetAll(filter, default);

        //Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WhenFilterIsEmpty_ReturnsAllSubscriptions()
    {
        //Arrange
        var subscriptions = Fixture.CreateMany<Subscription>(3).ToList();
        var subscriptionDtos = Fixture.CreateMany<SubscriptionDto>(3).ToList();

        var filter = new SubscriptionFilterDto();

        Repository.GetAll(Arg.Any<Expression<Func<Subscription, bool>>>(), default)
           .Returns(subscriptions);
        Mapper.Map<List<SubscriptionDto>>(subscriptions).Returns(subscriptionDtos);

        //Act
        var result = await Service.GetAll(filter, default);

        //Assert
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(3);
        result.ShouldBe(subscriptionDtos);
    }
}
