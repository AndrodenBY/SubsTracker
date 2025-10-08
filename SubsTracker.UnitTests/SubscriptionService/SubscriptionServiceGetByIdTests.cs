namespace SubsTracker.UnitTests.SubscriptionService;

public class SubscriptionServiceGetByIdTests : SubscriptionServiceTestsBase
{
    [Fact]
    public async Task GetById_WhenSubscriptionExists_ReturnsSubscriptionDto()
    {
        //Arrange
        var subscriptionEntity = _fixture.Create<Subscription>();

        var subscriptionDto = _fixture.Build<SubscriptionDto>()
            .With(subscription => subscription.Id, subscriptionEntity.Id)
            .With(subscription => subscription.Name, subscriptionEntity.Name)
            .With(subscription => subscription.Price, subscriptionEntity.Price)
            .With(subscription => subscription.DueDate, subscriptionEntity.DueDate)
            .Create();

        _repository.GetById(subscriptionEntity.Id, default)
            .Returns(subscriptionEntity);

        _mapper.Map<SubscriptionDto>(subscriptionEntity)
            .Returns(subscriptionDto);

        //Act
        var result = await _service.GetById(subscriptionEntity.Id, default);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(subscriptionEntity.Id);
        result.Name.ShouldBe(subscriptionEntity.Name);
        result.Price.ShouldBe(subscriptionEntity.Price);
        result.DueDate.ShouldBe(subscriptionEntity.DueDate);

        await _repository.Received(1).GetById(subscriptionEntity.Id, default);
    }

    [Fact]
    public async Task GetById_WhenEmptyGuid_ReturnsNull()
    {
        //Arrange
        var emptyId = Guid.Empty;
        
        //Act
        var emptyIdResult = await _service.GetById(emptyId, default);
        
        //Assert
        emptyIdResult.ShouldBeNull();
    }

    [Fact]
    public async Task GetById_WhenSubscriptionDoesNotExist_ReturnsNull()
    {
        //Arrange
        var fakeId = Guid.NewGuid();
        
        //Act
        var fakeIdResult = await _service.GetById(fakeId, default);
        
        //Assert
        fakeIdResult.ShouldBeNull();
    }
}
