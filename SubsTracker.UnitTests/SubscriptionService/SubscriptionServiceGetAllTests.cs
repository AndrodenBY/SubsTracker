namespace SubsTracker.UnitTests.SubscriptionService;

public class SubscriptionServiceGetAllTests : SubscriptionServiceTestsBase
{
    [Fact]
    public async Task GetAll_WhenFilteredByName_ReturnsCorrectSubscription()
    {
        //Arrange
        var subscriptionToFind = _fixture.Create<Subscription>();
        var subscriptionDto = new SubscriptionDto { Id = subscriptionToFind.Id, Name = subscriptionToFind.Name};
        var filter = new SubscriptionFilterDto { Name = subscriptionToFind.Name };

        _repository.GetAll(Arg.Any<Expression<Func<Subscription, bool>>>(), default)
            .Returns(new List<Subscription> { subscriptionToFind });
        _mapper.Map<List<SubscriptionDto>>(Arg.Any<List<Subscription>>()).Returns(new List<SubscriptionDto> { subscriptionDto });

        //Act
        var result = await _service.GetAll(filter, default);
        
        //Assert
        result.ShouldNotBeNull();
        result.ShouldHaveSingleItem();
        result.Single().Name.ShouldBe(subscriptionToFind.Name);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentName_ReturnsEmptyList()
    {
        // Arrange
        var userGroupToFind = _fixture.Create<UserGroup>();
        var userGroupDto = new UserGroupDto { Id = userGroupToFind.Id, Name = userGroupToFind.Name};
        var filter = new SubscriptionFilterDto { Name = "LetThatSinkIn" };

        _repository.GetAll(Arg.Any<Expression<Func<Subscription, bool>>>(), default)
            .Returns(new List<Subscription>());
        _mapper.Map<List<SubscriptionDto>>(Arg.Any<List<Subscription>>()).Returns(new List<SubscriptionDto>());
        
        // Act
        var result = await _service.GetAll(filter, default);
        
        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WhenNoSubscriptions_ReturnsEmptyList()
    {
        //Arrange
        var filter = new SubscriptionFilterDto();
        
        _repository.GetAll(Arg.Any<Expression<Func<Subscription, bool>>>(), default)
            .Returns(new List<Subscription>());
        _mapper.Map<List<SubscriptionDto>>(Arg.Any<List<Subscription>>()).Returns(new List<SubscriptionDto>());
        
        //Act
        var result = await _service.GetAll(filter, default);
        
        //Assert
        result.ShouldBeEmpty();
    }
    
    [Fact]
    public async Task GetAll_WhenFilterIsEmpty_ReturnsAllSubscriptions()
    {
        //Arrange
        var subscriptions = _fixture.CreateMany<Subscription>(3).ToList();
        var subscriptionDtos = _fixture.CreateMany<SubscriptionDto>(3).ToList();
        
        var filter = new SubscriptionFilterDto();

        _repository.GetAll(Arg.Any<Expression<Func<Subscription, bool>>>(), default)
            .Returns(subscriptions);
        _mapper.Map<List<SubscriptionDto>>(subscriptions).Returns(subscriptionDtos);

        //Act
        var result = await _service.GetAll(filter, default);

        //Assert
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(3);
        result.ShouldBe(subscriptionDtos);
    }
}
