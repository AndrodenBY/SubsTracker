using System.Net.Http.Headers;
using MassTransit.Testing;
using SubsTracker.IntegrationTests.Configuration.WebApplicationFactory;
using SubsTracker.Messaging.Contracts;

namespace SubsTracker.IntegrationTests.Subscription;

[Collection("SequentialIntegrationTests")]
public class SubscriptionsControllerTests : IClassFixture<TestsWebApplicationFactory>
{
    private readonly TestsWebApplicationFactory _factory;
    private readonly SubscriptionTestsAssertionHelper _assertHelper;
    private readonly HttpClient _client;
    private readonly SubscriptionTestsDataSeedingHelper _dataSeedingHelper;
    private readonly ITestHarness _harness;

    public SubscriptionsControllerTests(TestsWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestAuthScheme");
        _dataSeedingHelper = new SubscriptionTestsDataSeedingHelper(factory);
        _assertHelper = new SubscriptionTestsAssertionHelper(factory);
        _harness = factory.Services.GetRequiredService<ITestHarness>();
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }

    [Fact]
    public async Task GetById_ShouldReturnCorrectSubscription()
    {
        //Arrange
        var seed = await _dataSeedingHelper.AddSeedData();
        var subscription = seed.Subscriptions.First();
        var client = _factory.CreateAuthenticatedClient();

        //Act
        var response = await client.GetAsync($"{EndpointConst.Subscription}/{subscription.Id}");

        //Assert
        await _assertHelper.GetByIdValidAssert(response, subscription);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByName_ReturnsOnlyMatchingSubscription()
    {
        //Arrange
        await _dataSeedingHelper.AddSeedUserWithSubscriptions("Target Subscription", "Unrelated App");
        var url = $"{EndpointConst.Subscription}?Name=Target Subscription&PageNumber=1&PageSize=10";

        //Act
        var response = await _client.GetAsync(url);

        //Assert
        await _assertHelper.GetAllValidAssert(response, "Target Subscription");
    }

    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentName_ReturnsEmptyList()
    {
        //Arrange
        await _dataSeedingHelper.AddSeedData();
        var nonExistentName = "NonExistentFilter";
        var url = $"{EndpointConst.Subscription}?Name={nonExistentName}&PageNumber=1&PageSize=10";

        //Act
        var response = await _client.GetAsync(url);

        //Assert
        await _assertHelper.GetAllInvalidAssert(response);
    }

    [Fact]
    public async Task Create_WhenValidData_ReturnsCreatedSubscription()
    {
        //Arrange
        var subscriptionDto = _dataSeedingHelper.AddCreateSubscriptionDto();
        await _dataSeedingHelper.AddSeedUserOnly();
        var client = _factory.CreateAuthenticatedClient();
        
        //Act
        var response = await client.PostAsJsonAsync($"{EndpointConst.Subscription}", subscriptionDto);

        //Assert
        await _assertHelper.CreateValidAssert(response);
    }

    [Fact]
    public async Task Update_WhenValidData_ReturnsUpdatedSubscription()
    {
        //Arrange
        var dataSeedObject = await _dataSeedingHelper.AddSeedData();
        var subscription = dataSeedObject.Subscriptions.First();
        var updateDto = _dataSeedingHelper.AddUpdateSubscriptionDto(subscription.Id);
        
        var client = _factory.CreateAuthenticatedClient();
    
        //Act
        var response = await client.PutAsJsonAsync($"{EndpointConst.Subscription}", updateDto);

        //Assert
        await _assertHelper.UpdateValidAssert(response, updateDto.Name);
    }
    
    [Fact]
    public async Task CancelSubscription_ShouldPublishSubscriptionCanceledEvent()
    {
        //Arrange
        var seedData = await _dataSeedingHelper.AddSeedUserWithSubscriptions("Streaming Service");
        var subscription = seedData.Subscriptions.First();
        
        var client = _factory.CreateAuthenticatedClient();

        //Act
        var response = await client.PatchAsync($"{EndpointConst.Subscription}/{subscription.Id}/cancel", null);

        //Assert
        await _assertHelper.CancelSubscriptionValidAssert(response, subscription);
    
        (await _harness.Published.Any<SubscriptionCanceledEvent>(x =>
            x.Context.Message.Id == subscription.Id)).ShouldBeTrue();
    }
    
    [Fact]
    public async Task RenewSubscription_ShouldPublishSubscriptionRenewedEvent()
    {
        //Arrange
        var seed = await _dataSeedingHelper.AddSeedData();
        var subscription = seed.Subscriptions.First();
        var monthsToRenew = 3;

        //Act
        var response = await _client.PatchAsync(
            $"{EndpointConst.Subscription}/{subscription.Id}/renew?monthsToRenew={monthsToRenew}",
            null);

        //Assert
        await _assertHelper.RenewSubscriptionValidAssert(
            response,
            subscription,
            subscription.DueDate.AddMonths(monthsToRenew)
        );
        
        Assert.True(_harness.Published.Select<SubscriptionRenewedEvent>().Any(), "Expected a SubscriptionRenewedEvent to be published"
        );
    }

    [Fact]
    public async Task GetUpcomingBills_WhenAnySubscriptionsAreDue_ShouldReturnOnlyUpcomingSubscriptions()
    {
        //Arrange
        var client = _factory.CreateAuthenticatedClient();
        var seedData = await _dataSeedingHelper.AddSeedUserWithUpcomingAndNonUpcomingSubscriptions();
        var upcoming = seedData.Subscriptions
            .Where(s => s.DueDate >= DateOnly.FromDateTime(DateTime.Now) && s.DueDate <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)))
            .OrderBy(s => s.DueDate)
            .First();
        
        //Act
        var response = await client.GetAsync($"{EndpointConst.Subscription}/bills/users");
        
        //Assert
        await _assertHelper.GetUpcomingBillsValidAssert(response, upcoming);
    }
}
