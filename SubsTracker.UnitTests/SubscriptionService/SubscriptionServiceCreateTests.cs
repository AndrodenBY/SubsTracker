namespace SubsTracker.UnitTests.SubscriptionService;

public class SubscriptionServiceCreateTests : SubscriptionServiceTestsBase
{
    [Fact]
    public async Task Create_WhenValidModel_ReturnsCreatedSubscription()
    {
        //Arrange
        var auth0Id = Fixture.Create<string>();
        var createDto = Fixture.Create<CreateSubscriptionDto>();
        var existingUser = Fixture.Create<User>();

        var subscriptionEntity = Fixture.Build<Subscription>()
            .With(s => s.Name, createDto.Name)
            .With(s => s.UserId, existingUser.Id)
            .Create();

        var subscriptionDto = Fixture.Build<SubscriptionDto>()
            .With(s => s.Name, createDto.Name)
            .Create();

        UserRepository.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>())
            .Returns(existingUser);

        SubscriptionRepository.GetAll(Arg.Any<Expression<Func<Subscription, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Subscription>());

        Mapper.Map<Subscription>(createDto)
            .Returns(subscriptionEntity);

        SubscriptionRepository.Create(subscriptionEntity, Arg.Any<CancellationToken>())
            .Returns(subscriptionEntity);

        Mapper.Map<SubscriptionDto>(subscriptionEntity)
            .Returns(subscriptionDto);

        //Act
        var result = await Service.Create(auth0Id, createDto, default);

        //Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(createDto.Name);

        await UserRepository.Received(1).GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>());
    
        await SubscriptionRepository.Received(1).Create(
            Arg.Is<Subscription>(s => s.Name == createDto.Name && s.UserId == existingUser.Id), 
            Arg.Any<CancellationToken>());

        await HistoryRepository.Received(1).Create(
            subscriptionEntity.Id, 
            SubscriptionAction.Activate, 
            createDto.Price, 
            Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Create_WhenSubscriptionAlreadyExists_ThrowsPolicyViolationException()
    {
        //Arrange
        var auth0Id = Fixture.Create<string>();
        var createDto = Fixture.Create<CreateSubscriptionDto>();
        var existingUser = Fixture.Create<User>();

        var existingSubscription = Fixture.Build<Subscription>()
            .With(s => s.Name, createDto.Name)
            .With(s => s.UserId, existingUser.Id)
            .With(s => s.Active, true)
            .Create();

        UserRepository.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>())
            .Returns(existingUser);
        
        SubscriptionRepository.GetByPredicate(Arg.Any<Expression<Func<Subscription, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(existingSubscription);

        //Act & Assert
        var exception = await Assert.ThrowsAsync<PolicyViolationException>(() => 
            Service.Create(auth0Id, createDto, default));

        exception.Message.ShouldContain(createDto.Name);

        await SubscriptionRepository.DidNotReceive().Create(Arg.Any<Subscription>(), Arg.Any<CancellationToken>());
        await HistoryRepository.DidNotReceive().Create(Arg.Any<Guid>(), Arg.Any<SubscriptionAction>(), Arg.Any<decimal?>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Create_WhenUserNotExists_ReturnsNull()
    {
        //Arrange
        var createDto = Fixture.Create<CreateSubscriptionDto>();

        //Act & Assert
        await Should.ThrowAsync<UnknownIdentifierException>(async () =>
            await Service.Create(string.Empty, createDto, default));
    }
}
