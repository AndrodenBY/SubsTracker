using MassTransit.Testing;
using SubsTracker.Messaging.Contracts;

namespace SubsTracker.IntegrationTests.Subscription;

public class SubscriptionsControllerTests : IClassFixture<TestsWebApplicationFactory>
{
    private readonly SubscriptionTestsAssertionHelper _assertHelper;
    private readonly HttpClient _client;
    private readonly SubscriptionTestsDataSeedingHelper _dataSeedingHelper;
    private readonly ITestHarness _harness;

    public SubscriptionsControllerTests(TestsWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _dataSeedingHelper = new SubscriptionTestsDataSeedingHelper(factory);
        _assertHelper = new SubscriptionTestsAssertionHelper(factory);
        _harness = factory.Services.GetRequiredService<ITestHarness>();
    }
    
    public async Task InitializeAsync()
    {
        await _harness.Start();
    }

    public async Task DisposeAsync()
    {
        await _harness.Stop();
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
        await _assertHelper.GetByIdValidAssert(response, subscription);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByName_ReturnsOnlyMatchingSubscription()
    {
        //Arrange
        await _dataSeedingHelper.AddSeedUserWithSubscriptions("Target Subscription", "Unrelated App");

        //Act
        var response = await _client.GetAsync($"{EndpointConst.Subscription}?Name=Target Subscription");

        //Assert
        await _assertHelper.GetAllValidAssert(response, "Target Subscription");
    }

    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentName_ReturnsEmptyList()
    {
        //Arrange
        await _dataSeedingHelper.AddSeedData();
        var nonExistentName = "NonExistentFilter";

        //Act
        var response = await _client.GetAsync($"{EndpointConst.Subscription}?Name={nonExistentName}");

        //Assert
        await _assertHelper.GetAllInvalidAssert(response);
    }

    [Fact]
    public async Task Create_WhenValidData_ReturnsCreatedSubscription()
    {
        //Arrange
        var subscriptionDto = _dataSeedingHelper.AddCreateSubscriptionDto();
        var dataSeedObject = await _dataSeedingHelper.AddSeedUserOnly();

        //Act
        var response =
            await _client.PostAsJsonAsync($"{EndpointConst.Subscription}/{dataSeedObject.User.Id}", subscriptionDto);

        //Assert
        await _assertHelper.CreateValidAssert(response);
    }

    [Fact]
    public async Task Update_WhenValidData_ReturnsUpdatedSubscription()
    {
        //Arrange
        var dataSeedObject = await _dataSeedingHelper.AddSeedData();
        var subscription = dataSeedObject.Subscriptions.FirstOrDefault();
        var updateDto = _dataSeedingHelper.AddUpdateSubscriptionDto(subscription.Id);

        //Act
        var response =
            await _client.PutAsJsonAsync($"{EndpointConst.Subscription}/{dataSeedObject.User.Id}", updateDto);

        //Assert
        await _assertHelper.UpdateValidAssert(response, subscription.Id, updateDto.Name);
    }

    [Fact]
    public async Task CancelSubscription_WhenValidData_ReturnsCancelledSubscription()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddSeedUserWithSubscriptions("Streaming Service");
        var subscription = seedData.Subscriptions.FirstOrDefault();

        //Act
        var response =
            await _client.PatchAsync($"{EndpointConst.Subscription}/{subscription.Id}/cancel?userId={seedData.User.Id}",
                null);

        //Assert
        await _assertHelper.CancelSubscriptionValidAssert(response, subscription);
    }

    [Fact]
    public async Task RenewSubscription_ShouldPublishSubscriptionRenewedEvent()
    {
        // Arrange
        var seed = await _dataSeedingHelper.AddSeedData();
        var subscription = seed.Subscriptions.First();

        var monthsToRenew = 3;

        // Act
        var response = await _client.PatchAsync(
            $"{EndpointConst.Subscription}/{subscription.Id}/renew?monthsToRenew={monthsToRenew}", 
            null);

        // Assert
        await _assertHelper.RenewSubscriptionValidAssert(
            response, 
            subscription, 
            subscription.DueDate.AddMonths(monthsToRenew)
        );

        Assert.True(_harness.Published.Select<SubscriptionRenewedEvent>().Any(), 
            "Expected a SubscriptionRenewedEvent to be published");
    }


    [Fact]
    public async Task GetUpcomingBills_WhenAnySubscriptionsAreDue_ShouldReturnOnlyUpcomingSubscriptions()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddSeedUserWithUpcomingAndNonUpcomingSubscriptions();
        var upcoming = seedData.Subscriptions.First(s => s.DueDate <= DateOnly.FromDateTime(DateTime.Today.AddDays(7)));

        //Act
        var response = await _client.GetAsync($"{EndpointConst.Subscription}/bills/users/{seedData.User.Id}");

        //Assert
        await _assertHelper.GetUpcomingBillsValidAssert(response, upcoming);
    }
}
