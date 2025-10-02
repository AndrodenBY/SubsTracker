namespace SubsTracker.IntegrationTests.Subscription;

public class SubscriptionsControllerTests : IClassFixture<TestsWebApplicationFactory>, IAsyncDisposable
{
    private readonly TestsWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly SubscriptionTestHelper _helper;

    public SubscriptionsControllerTests(TestsWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _helper = new SubscriptionTestHelper(factory);
    }
    
    [Fact]
    public async Task GetById_ShouldReturnCorrectSubscription()
    {
        //Arrange
        var dataSeedObject = await _helper.AddSeedData();
        var subscription = dataSeedObject.Subscriptions.FirstOrDefault();

        //Act
        var response = await _client.GetAsync($"{EndpointConst.Subscription}/{subscription.Id}");
        
        //Assert
        await _helper.GetByIdHappyPathAssert(response, subscription);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByName_ReturnsOnlyMatchingSubscription()
    {
        //Arrange
        await _helper.ClearTestDataWithRelations();
        var seedData = await _helper.AddSeedUserWithSubscriptions("Target Subscription", "Unrelated App");
        
        //Act
        var response = await _client.GetAsync($"{EndpointConst.Subscription}?Name=Target Subscription");
        
        //Assert
        await _helper.GetAllHappyPathAssert(response, "Target Subscription");
    }
    
    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentName_ReturnsEmptyList()
    {
        //Arrange
        var seedData = await _helper.AddSeedData();
        var nonExistentName = "NonExistentFilter";

        //Act
        var response = await _client.GetAsync($"{EndpointConst.Subscription}?Name={nonExistentName}");
        
        //Assert
        await _helper.GetAllSadPathAssert(response);
    }
    
    [Fact]
    public async Task Create_WhenValidData_ReturnsCreatedSubscription()
    {
        //Arrange
        var subscriptionDto = await _helper.AddCreateSubscriptionDto();
        var dataSeedObject = await _helper.AddSeedUserOnly();
    
        //Act
        var response = await _client.PostAsJsonAsync($"{EndpointConst.Subscription}/{dataSeedObject.User.Id}", subscriptionDto);
        
        //Assert
        await _helper.CreateHappyPathAssert(response);
    }
    
    [Fact]
    public async Task Update_WhenValidData_ReturnsUpdatedSubscription()
    {
        //Arrange
        var dataSeedObject = await _helper.AddSeedData();
        var subscription = dataSeedObject.Subscriptions.FirstOrDefault();
        var updateDto = await _helper.AddUpdateSubscriptionDto(subscription.Id);
            
        //Act
        var response = await _client.PutAsJsonAsync($"{EndpointConst.Subscription}/{dataSeedObject.User.Id}", updateDto);
        
        //Assert
        await _helper.UpdateHappyPathAssert(response, subscription.Id, updateDto.Name);
    }
    
    [Fact]
    public async Task CancelSubscription_WhenValidData_ReturnsCancelledSubscription()
    {
        //Arrange
        await _helper.ClearTestDataWithRelations();
        var seedData = await _helper.AddSeedUserWithSubscriptions("Streaming Service");
        var subscription = seedData.Subscriptions.FirstOrDefault();
        
        //Act
        var response = await _client.PatchAsync($"{EndpointConst.Subscription}/{subscription.Id}/cancel?userId={seedData.User.Id}", null);
        
        //Assert
        await _helper.CancelSubscriptionHappyPathAssert(response, subscription);
    }

    [Fact]
    public async Task RenewSubscription_WhenValidData_UpdatesDueDateAndActivates()
    {
        //Arrange
        await _helper.ClearTestDataWithRelations();

        var seed = await _helper.AddSeedData();
        var subscription = seed.Subscriptions.FirstOrDefault();

        var monthsToRenew = 3;
        var expectedDueDate = subscription.DueDate.AddMonths(monthsToRenew);

        //Act
        var response = await _client.PatchAsync($"{EndpointConst.Subscription}/{subscription.Id}/renew?monthsToRenew={monthsToRenew}", null);

        //Assert
        await _helper.RenewSubscriptionHappyPathAssert(response, subscription, expectedDueDate);
    }

    [Fact]
    public async Task GetUpcomingBills_WhenAnySubscriptionsAreDue_ShouldReturnOnlyUpcomingSubscriptions()
    {
        //Arrange
        await _helper.ClearTestDataWithRelations();

        var seedData = await _helper.AddSeedUserWithUpcomingAndNonUpcomingSubscriptions();
        var upcoming = seedData.Subscriptions.First(s => s.DueDate <= DateOnly.FromDateTime(DateTime.Today.AddDays(7)));

        //Act
        var response = await _client.GetAsync($"{EndpointConst.Subscription}/bills/users/{seedData.User.Id}");

        //Assert
        await _helper.GetUpcomingBillsHappyPathAssert(response, upcoming);
    }
    
    public async ValueTask DisposeAsync()
    {
        await _helper.ClearTestData();
        _client.Dispose();
        _factory.Dispose();
    }
}
