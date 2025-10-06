namespace SubsTracker.IntegrationTests.Subscription;

public class SubscriptionsControllerTests : IClassFixture<TestsWebApplicationFactory>, IAsyncDisposable
{
    private readonly TestsWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly SubscriptionTestsDataSeedingHelper _dataSeedingHelper;
    private readonly SubscriptionTestsAssertionHelper _assertHelper;

    public SubscriptionsControllerTests(TestsWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _dataSeedingHelper = new SubscriptionTestsDataSeedingHelper(factory);
        _assertHelper = new SubscriptionTestsAssertionHelper(factory);
    }
    
    [Fact]
    public async Task GetById_ShouldReturnCorrectSubscription()
    {
        //Arrange
        var dataSeedObject = await _dataSeedingHelper.AddSeedData();
        var subscription = dataSeedObject.Subscriptions.FirstOrDefault();

        //Act
        var response = await _client.GetAsync($"{EndpointConst.Subscription}/{subscription.Id}");
        
        //Assert
        await _assertHelper.GetByIdHappyPathAssert(response, subscription);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByName_ReturnsOnlyMatchingSubscription()
    {
        //Arrange
        await _dataSeedingHelper.ClearTestDataWithRelations();
        var seedData = await _dataSeedingHelper.AddSeedUserWithSubscriptions("Target Subscription", "Unrelated App");
        
        //Act
        var response = await _client.GetAsync($"{EndpointConst.Subscription}?Name=Target Subscription");
        
        //Assert
        await _assertHelper.GetAllHappyPathAssert(response, "Target Subscription");
    }
    
    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentName_ReturnsEmptyList()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddSeedData();
        var nonExistentName = "NonExistentFilter";

        //Act
        var response = await _client.GetAsync($"{EndpointConst.Subscription}?Name={nonExistentName}");
        
        //Assert
        await _assertHelper.GetAllSadPathAssert(response);
    }
    
    [Fact]
    public async Task Create_WhenValidData_ReturnsCreatedSubscription()
    {
        //Arrange
        var subscriptionDto = await _dataSeedingHelper.AddCreateSubscriptionDto();
        var dataSeedObject = await _dataSeedingHelper.AddSeedUserOnly();
    
        //Act
        var response = await _client.PostAsJsonAsync($"{EndpointConst.Subscription}/{dataSeedObject.User.Id}", subscriptionDto);
        
        //Assert
        await _assertHelper.CreateHappyPathAssert(response);
    }
    
    [Fact]
    public async Task Update_WhenValidData_ReturnsUpdatedSubscription()
    {
        //Arrange
        var dataSeedObject = await _dataSeedingHelper.AddSeedData();
        var subscription = dataSeedObject.Subscriptions.FirstOrDefault();
        var updateDto = await _dataSeedingHelper.AddUpdateSubscriptionDto(subscription.Id);
            
        //Act
        var response = await _client.PutAsJsonAsync($"{EndpointConst.Subscription}/{dataSeedObject.User.Id}", updateDto);
        
        //Assert
        await _assertHelper.UpdateHappyPathAssert(response, subscription.Id, updateDto.Name);
    }
    
    [Fact]
    public async Task CancelSubscription_WhenValidData_ReturnsCancelledSubscription()
    {
        //Arrange
        await _dataSeedingHelper.ClearTestDataWithRelations();
        var seedData = await _dataSeedingHelper.AddSeedUserWithSubscriptions("Streaming Service");
        var subscription = seedData.Subscriptions.FirstOrDefault();
        
        //Act
        var response = await _client.PatchAsync($"{EndpointConst.Subscription}/{subscription.Id}/cancel?userId={seedData.User.Id}", null);
        
        //Assert
        await _assertHelper.CancelSubscriptionHappyPathAssert(response, subscription);
    }

    [Fact]
    public async Task RenewSubscription_WhenValidData_UpdatesDueDateAndActivates()
    {
        //Arrange
        await _dataSeedingHelper.ClearTestDataWithRelations();

        var seed = await _dataSeedingHelper.AddSeedData();
        var subscription = seed.Subscriptions.FirstOrDefault();

        var monthsToRenew = 3;
        var expectedDueDate = subscription.DueDate.AddMonths(monthsToRenew);

        //Act
        var response = await _client.PatchAsync($"{EndpointConst.Subscription}/{subscription.Id}/renew?monthsToRenew={monthsToRenew}", null);

        //Assert
        await _assertHelper.RenewSubscriptionHappyPathAssert(response, subscription, expectedDueDate);
    }

    [Fact]
    public async Task GetUpcomingBills_WhenAnySubscriptionsAreDue_ShouldReturnOnlyUpcomingSubscriptions()
    {
        //Arrange
        await _dataSeedingHelper.ClearTestDataWithRelations();

        var seedData = await _dataSeedingHelper.AddSeedUserWithUpcomingAndNonUpcomingSubscriptions();
        var upcoming = seedData.Subscriptions.First(s => s.DueDate <= DateOnly.FromDateTime(DateTime.Today.AddDays(7)));

        //Act
        var response = await _client.GetAsync($"{EndpointConst.Subscription}/bills/users/{seedData.User.Id}");

        //Assert
        await _assertHelper.GetUpcomingBillsHappyPathAssert(response, upcoming);
    }
    
    public async ValueTask DisposeAsync()
    {
        await _dataSeedingHelper.ClearTestData();
        _client.Dispose();
        _factory.Dispose();
    }
}
