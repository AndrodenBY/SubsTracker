namespace SubsTracker.UnitTests.SubscriptionService;

public class SubscriptionServiceGetByIdTests : SubscriptionServiceTestsBase
{
    [Fact]
    public async Task GetById_WhenSubscriptionExists_ReturnsSubscriptionDto()
    {
        // Arrange
        var subscriptionEntity = _fixture.Create<Subscription>();

        var subscriptionDto = new SubscriptionDto
        {
            Id = subscriptionEntity.Id,
            Name = subscriptionEntity.Name,
            Price = subscriptionEntity.Price,
            DueDate = subscriptionEntity.DueDate
        };

        _repository.GetById(subscriptionEntity.Id, default)
            .Returns(subscriptionEntity);

        _mapper.Map<SubscriptionDto>(subscriptionEntity)
            .Returns(subscriptionDto);

        // Act
        var result = await _service.GetById(subscriptionEntity.Id, default);

        // Assert
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
