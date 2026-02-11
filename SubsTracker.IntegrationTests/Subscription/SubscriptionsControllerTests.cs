using System.Net.Http.Headers;
using MassTransit.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.IntegrationTests.Configuration.WebApplicationFactory;
using SubsTracker.Messaging.Contracts;
using SubsTracker.Messaging.Interfaces;
using SubsTracker.Messaging.Services;
using Xunit;

namespace SubsTracker.IntegrationTests.Subscription;

public class SubscriptionsControllerTests :
    IClassFixture<TestsWebApplicationFactory>,
    IAsyncLifetime
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
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("TestAuthScheme");

        _dataSeedingHelper = new SubscriptionTestsDataSeedingHelper(factory);
        _assertHelper = new SubscriptionTestsAssertionHelper(factory);
        _harness = factory.Services.GetRequiredService<ITestHarness>();
    }
    
    public async Task InitializeAsync() => await _harness.Start();

    public async Task DisposeAsync() => await _harness.Stop();

    [Fact]
    public async Task GetById_ShouldReturnCorrectSubscription()
    {
        var seed = await _dataSeedingHelper.AddSeedData();
        var subscription = seed.Subscriptions.First();

        var client = _factory.CreateAuthenticatedClient();

        var response = await client.GetAsync($"{EndpointConst.Subscription}/{subscription.Id}");

        await _assertHelper.GetByIdValidAssert(response, subscription);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByName_ReturnsOnlyMatchingSubscription()
    {
        await _dataSeedingHelper.AddSeedUserWithSubscriptions("Target Subscription", "Unrelated App");

        var response = await _client.GetAsync($"{EndpointConst.Subscription}?Name=Target Subscription");

        await _assertHelper.GetAllValidAssert(response, "Target Subscription");
    }

    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentName_ReturnsEmptyList()
    {
        await _dataSeedingHelper.AddSeedData();
        var nonExistentName = "NonExistentFilter";

        var response = await _client.GetAsync($"{EndpointConst.Subscription}?Name={nonExistentName}");

        await _assertHelper.GetAllInvalidAssert(response);
    }

    [Fact]
    public async Task Create_WhenValidData_ReturnsCreatedSubscription()
    {
        var subscriptionDto = _dataSeedingHelper.AddCreateSubscriptionDto();
        var dataSeedObject = await _dataSeedingHelper.AddSeedUserOnly();

        var response = await _client.PostAsJsonAsync(
            $"{EndpointConst.Subscription}/{dataSeedObject.User.Id}", subscriptionDto);

        await _assertHelper.CreateValidAssert(response);
    }

    [Fact]
    public async Task Update_WhenValidData_ReturnsUpdatedSubscription()
    {
        var dataSeedObject = await _dataSeedingHelper.AddSeedData();
        var subscription = dataSeedObject.Subscriptions.First();
        var updateDto = _dataSeedingHelper.AddUpdateSubscriptionDto(subscription.Id);

        var response = await _client.PutAsJsonAsync(
            $"{EndpointConst.Subscription}/{dataSeedObject.User.Id}", updateDto);

        await _assertHelper.UpdateValidAssert(response, subscription.Id, updateDto.Name);
    }
    
    [Fact]
    public async Task CancelSubscription_ShouldPublishSubscriptionCanceledEvent()
    {
        var seedData = await _dataSeedingHelper.AddSeedUserWithSubscriptions("Streaming Service");
        var subscription = seedData.Subscriptions.First();

        var client = _factory.CreateAuthenticatedClient();
        
        var response = await client.PatchAsync(
            $"{EndpointConst.Subscription}/{subscription.Id}/cancel?userId={seedData.User.Id}",
            null);
        
        await _assertHelper.CancelSubscriptionValidAssert(response, subscription);
        
        (await _harness.Published.Any<SubscriptionCanceledEvent>(x =>
                x.Context.Message.Id == subscription.Id))
            .ShouldBeTrue();
    }

    
    [Fact]
    public async Task RenewSubscription_ShouldPublishSubscriptionRenewedEvent()
    {
        var seed = await _dataSeedingHelper.AddSeedData();
        var subscription = seed.Subscriptions.First();

        var monthsToRenew = 3;

        var response = await _client.PatchAsync(
            $"{EndpointConst.Subscription}/{subscription.Id}/renew?monthsToRenew={monthsToRenew}",
            null);

        await _assertHelper.RenewSubscriptionValidAssert(
            response,
            subscription,
            subscription.DueDate.AddMonths(monthsToRenew)
        );
        
        Assert.True(
            _harness.Published.Select<SubscriptionRenewedEvent>().Any(),
            "Expected a SubscriptionRenewedEvent to be published"
        );
    }

    [Fact]
    public async Task GetUpcomingBills_WhenAnySubscriptionsAreDue_ShouldReturnOnlyUpcomingSubscriptions()
    {
        var client = _factory.CreateAuthenticatedClient();

        var seedData = await _dataSeedingHelper.AddSeedUserWithUpcomingAndNonUpcomingSubscriptions();

        var upcoming = seedData.Subscriptions
            .Where(s => s.DueDate >= DateOnly.FromDateTime(DateTime.Now)
                        && s.DueDate <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)))
            .OrderBy(s => s.DueDate)
            .First();
        
        var response = await client.GetAsync(
            $"{EndpointConst.Subscription}/bills/users/{seedData.User.Id}");
        
        await _assertHelper.GetUpcomingBillsValidAssert(response, upcoming);
    }
}
