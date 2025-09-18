namespace SubsTracker.UnitTests.SubscriptionService;

public class SubscriptionServiceGetByIdTests : SubscriptionServiceTestsBase
{
    private readonly Guid _subscriptionId;
    private readonly Subscription _subscriptionEntity;
    private readonly SubscriptionDto _subscriptionDto;
    
    public SubscriptionServiceGetByIdTests()
    {
        _subscriptionId = Guid.NewGuid();
        _subscriptionEntity = new Subscription
        {
            Id = _subscriptionId, 
            Name = "Test Subscription", 
            Price = 100.00m, 
            DueDate = new DateOnly(2025, 12, 1), 
            Active = true
        };
        _subscriptionDto = new SubscriptionDto
        {
            Id = _subscriptionId, 
            Name = "Test Subscription", 
            Price = 100.00m, 
            DueDate = new DateOnly(2025, 12, 1),
        };
        
        _repository.GetById(_subscriptionId, default)
            .Returns(Task.FromResult<Subscription?>(_subscriptionEntity));
        
        _mapper.Map<SubscriptionDto>(_subscriptionEntity)
            .Returns(_subscriptionDto);
    }

    [Fact]
    public async Task GetById_ShouldReturnSubscriptionDto_WhenSubscriptionExists()
    {
        //Act
        var result = await _service.GetById(_subscriptionId, default);
        
        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(_subscriptionEntity.Id);
        result.Name.ShouldBe(_subscriptionEntity.Name);
        result.Price.ShouldBe(_subscriptionEntity.Price);
        result.DueDate.ShouldBe(_subscriptionEntity.DueDate);
    }

    [Fact]
    public async Task GetById_ShouldReturnNull_WhenEmptyGuid()
    {
        //Arrange
        var emptyId = Guid.Empty;
        
        //Act
        var emptyIdResult = await _service.GetById(emptyId, default);
        
        //Assert
        emptyIdResult.ShouldBeNull();
    }

    [Fact]
    public async Task GetById_ShouldReturnNull_WhenSubscriptionDoesNotExist()
    {
        //Arrange
        var fakeId = Guid.NewGuid();
        
        //Act
        var fakeIdResult = await _service.GetById(fakeId, default);
        
        //Assert
        fakeIdResult.ShouldBeNull();
    }

    [Fact]
    public async Task GetById_WhenCalled_CallsRepositoryExactlyOnce()
    {
        //Act
        await _service.GetById(_subscriptionId, default);
        
        //Assert
        await _repository.Received(1).GetById(_subscriptionId, default);
        _mapper.Received(1).Map<SubscriptionDto>(_subscriptionEntity);
    }
}
