using System.Net;
using System.Net.Http.Json;
using Allure.Net.Commons;
using Allure.Xunit.Attributes;
using MassTransit.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SubsTracker.API.ViewModel;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.DAL;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Pagination;
using SubsTracker.IntegrationTests.Configuration;
using SubsTracker.IntegrationTests.Configuration.ServiceConfigs;
using SubsTracker.IntegrationTests.Constants;
using SubsTracker.IntegrationTests.Helpers;
using SubsTracker.Messaging.Contracts;

namespace SubsTracker.IntegrationTests.Subscription;

[Collection("Subscription API")]
[AllureSuite("Integration Tests")]
[AllureFeature("Subscription Management")]
public class SubscriptionsControllerTests : IClassFixture<TestsWebApplicationFactory>
{
    private readonly TestsWebApplicationFactory _factory;
    private readonly SubscriptionTestsDataSeedingHelper _dataSeedingHelper;
    private readonly ITestHarness _harness;

    public SubscriptionsControllerTests(TestsWebApplicationFactory factory)
    {
        
        _factory = factory;
        _dataSeedingHelper = new SubscriptionTestsDataSeedingHelper(factory);
        _harness = factory.Services.GetRequiredService<ITestHarness>();
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }

    [Fact]
    [AllureSuite("Subscription API")]
    [AllureFeature("Information")]
    [AllureStory("Get Subscription by ID")]
    [AllureDescription("Verifies that a specific subscription can be retrieved by its GUID and matches the seeded data")]
    public async Task GetById_ShouldReturnCorrectSubscription()
    {
        // Arrange
        SubscriptionEntity expected = null!;
        HttpClient client = null!;

        await AllureApi.Step("Arrange: Seed subscription data and prepare client", async () => {
            var seed = await _dataSeedingHelper.AddSeedData();
            expected = seed.Subscriptions.First();
            client = _factory.CreateAuthenticatedClient();
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step($"Act: GET request for subscription ID: {expected.Id}", async () => {
            response = await client.GetAsync($"{EndpointConst.Subscription}/{expected.Id}");
        });

        // Assert
        await AllureApi.Step("Assert: Validate response status and subscription details", async () => {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
            var result = await response.Content.ReadFromJsonAsync<SubscriptionViewModel>(TestHelperBase.DefaultJsonOptions);
            result.ShouldNotBeNull();
            result.ShouldSatisfyAllConditions(
                () => result.Id.ShouldBe(expected.Id),
                () => result.Name.ShouldBe(expected.Name),
                () => result.Price.ShouldBe(expected.Price),
                () => result.Type.ShouldBe(expected.Type)
            );
        });
    }

    [Fact]
    [AllureSuite("Subscription API")]
    [AllureFeature("Filtering")]
    [AllureStory("Filter by Name")]
    [AllureDescription("Verifies that the subscription list correctly filters results by name and excludes unrelated subscriptions")]
    public async Task GetAll_WhenFilteredByName_ReturnsOnlyMatchingSubscription()
    {
        // Arrange
        const string targetName = "Target Subscription";
        HttpClient client = null!;

        await AllureApi.Step("Arrange: Seed target and unrelated subscriptions", async () => {
            await _dataSeedingHelper.AddSeedUserWithSubscriptions(targetName, "Unrelated App");
            client = _factory.CreateAuthenticatedClient();
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step($"Act: GET request with name filter: {targetName}", async () => {
            response = await client.GetAsync($"{EndpointConst.Subscription}?Name={targetName}");
        });

        // Assert
        await AllureApi.Step("Assert: Verify only the matching subscription is returned", async () => {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
            var result = await response.Content.ReadFromJsonAsync<PaginatedList<SubscriptionViewModel>>(TestHelperBase.DefaultJsonOptions);
            result.ShouldNotBeNull();
            result.Items.ShouldHaveSingleItem();
            result.Items[0].Name.ShouldBe(targetName);
            result.Items.ShouldNotContain(x => x.Name == "Unrelated App");
        });
    }

    [Fact]
    [AllureSuite("Subscription API")]
    [AllureFeature("Filtering")]
    [AllureStory("Filter by Non-Existent Name")]
    [AllureDescription("Verifies that the API returns an empty list when no subscriptions match the provided name filter")]
    public async Task GetAll_WhenFilteredByNonExistentName_ReturnsEmptyList()
    {
        // Arrange
        const string nonExistentName = "NonExistentFilter";
        HttpClient client = null!;

        await AllureApi.Step("Arrange: Seed base data and prepare client", async () => {
            await _dataSeedingHelper.AddSeedData();
            client = _factory.CreateAuthenticatedClient();
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step($"Act: GET request for non-existent name: {nonExistentName}", async () => {
            response = await client.GetAsync($"{EndpointConst.Subscription}?Name={nonExistentName}");
        });

        // Assert
        await AllureApi.Step("Assert: Validate response is OK with 0 items", async () => {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<PaginatedList<SubscriptionViewModel>>(TestHelperBase.DefaultJsonOptions);
            result.ShouldNotBeNull();
            result.Items.ShouldBeEmpty();
            result.TotalCount.ShouldBe(0);
        });
    }

    [Fact]
    [AllureSuite("Subscription API")]
    [AllureFeature("Lifecycle")]
    [AllureStory("Create Subscription")]
    [AllureDescription("Verifies that a valid DTO creates a new subscription, persists it to the DB, and returns the view model")]
    public async Task Create_WhenValidData_ReturnsCreatedSubscription()
    {
        // Arrange
        CreateSubscriptionDto dto = null!;
        HttpClient client = null!;

        await AllureApi.Step("Arrange: Prepare DTO and seed authenticated user", async () => {
            dto = _dataSeedingHelper.AddCreateSubscriptionDto();
            var seededUser = await _dataSeedingHelper.AddSeedUserOnly();
            client = _factory.CreateAuthenticatedClient(seededUser.UserEntity.IdentityId);
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step("Act: POST request to create subscription", async () => {
            response = await client.PostAsJsonAsync(EndpointConst.Subscription, dto);
        });

        // Assert
        await AllureApi.Step("Assert: Validate response status and view model fields", async () => {
            response.StatusCode.ShouldBe(HttpStatusCode.OK); 
            
            var result = await response.Content.ReadFromJsonAsync<SubscriptionViewModel>(TestHelperBase.DefaultJsonOptions);
            result.ShouldNotBeNull();
            result.ShouldSatisfyAllConditions(
                () => result.Id.ShouldNotBe(Guid.Empty),
                () => result.Name.ShouldBe(dto.Name),
                () => result.Price.ShouldBe(dto.Price),
                () => result.DueDate.ShouldBe(dto.DueDate)
            );
        
            await AllureApi.Step("Assert: Verify subscription entity exists in database", async () => {
                var entity = await _dataSeedingHelper.FindEntityAsync<SubscriptionEntity>(result.Id);
                entity.ShouldNotBeNull();
                entity.Name.ShouldBe(dto.Name);
            });
        });
    }

    [Fact]
    [AllureSuite("Subscription API")]
    [AllureFeature("Lifecycle")]
    [AllureStory("Update Subscription")]
    [AllureDescription("Verifies that an existing subscription can be modified and changes are reflected in the database")]
    public async Task Update_WhenValidData_ReturnsUpdatedSubscription()
    {
        // Arrange
        SubscriptionEntity existing = null!;
        UpdateSubscriptionDto updateDto = null!;
        HttpClient client = null!;

        await AllureApi.Step("Arrange: Seed existing subscription and prepare update DTO", async () => {
            var seed = await _dataSeedingHelper.AddSeedData();
            existing = seed.Subscriptions.First();
            updateDto = _dataSeedingHelper.AddUpdateSubscriptionDto(existing.Id);
            client = _factory.CreateAuthenticatedClient();
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step($"Act: PUT request to update subscription {existing.Id}", async () => {
            response = await client.PutAsJsonAsync(EndpointConst.Subscription, updateDto);
        });

        // Assert
        await AllureApi.Step("Assert: Validate updated view model values", async () => {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
            var result = await response.Content.ReadFromJsonAsync<SubscriptionViewModel>(TestHelperBase.DefaultJsonOptions);
            result.ShouldNotBeNull();
            result.ShouldSatisfyAllConditions(
                () => result.Id.ShouldBe(existing.Id),
                () => result.Name.ShouldBe(updateDto.Name),
                () => result.Price.ShouldBe((decimal)updateDto.Price!)
            );
        });

        await AllureApi.Step("Assert: Verify database entity was updated correctly", async () => {
            var dbEntity = await _dataSeedingHelper.FindEntityAsync<SubscriptionEntity>(existing.Id);
            dbEntity.ShouldNotBeNull();
            dbEntity.Name.ShouldBe(updateDto.Name);
            dbEntity.Price.ShouldBe((decimal)updateDto.Price!);
        });
    }
    
    [Fact]
    [AllureSuite("Subscription API")]
    [AllureFeature("Lifecycle")]
    [AllureStory("Cancel Subscription")]
    [AllureDescription("Verifies that a subscription is marked inactive and a cancellation event is published to the bus")]
    public async Task CancelSubscription_ShouldUpdateDatabaseAndPublishEvent()
    {
        // Arrange
        SubscriptionEntity expected = null!;
        HttpClient client = null!;

        await AllureApi.Step("Arrange: Seed streaming service subscription", async () => {
            var seed = await _dataSeedingHelper.AddSeedUserWithSubscriptions("Streaming Service");
            expected = seed.Subscriptions.First();
            client = _factory.CreateAuthenticatedClient();
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step($"Act: PATCH request to cancel subscription {expected.Id}", async () => {
            response = await client.PatchAsync($"{EndpointConst.Subscription}/{expected.Id}/cancel", null);
        });

        // Assert
        await AllureApi.Step("Assert: Validate response and database inactivity", async () => {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
            var result = await response.Content.ReadFromJsonAsync<SubscriptionViewModel>(TestHelperBase.DefaultJsonOptions);
            result.ShouldNotBeNull();
            result.Id.ShouldBe(expected.Id);
        
            var entity = await _dataSeedingHelper.FindEntityAsync<SubscriptionEntity>(expected.Id);
            entity.ShouldNotBeNull();
            entity.Active.ShouldBeFalse();
        });

        await AllureApi.Step("Assert: Verify SubscriptionCanceledEvent was published to MassTransit harness", async () => {
            var wasPublished = await _harness.Published.Any<SubscriptionCanceledEvent>(x => x.Context.Message.Id == expected.Id);
            wasPublished.ShouldBeTrue("The SubscriptionCanceledEvent was not published to the bus.");
        });
    }
    
    [Fact]
    [AllureSuite("Subscription API")]
    [AllureFeature("Lifecycle")]
    [AllureStory("Renew Subscription")]
    [AllureDescription("Verifies that a subscription's due date is extended correctly and a renewal event is published")]
    public async Task RenewSubscription_ShouldUpdateDueDateAndPublishEvent()
    {
        // Arrange
        SubscriptionEntity existing = null!;
        DateOnly expectedDate = default;
        const int monthsToRenew = 3;
        HttpClient client = null!;

        await AllureApi.Step($"Arrange: Seed subscription and calculate expected date (+{monthsToRenew} months)", async () => {
            var seed = await _dataSeedingHelper.AddSeedData();
            existing = seed.Subscriptions.First();
            expectedDate = existing.DueDate.AddMonths(monthsToRenew);
            client = _factory.CreateAuthenticatedClient();
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step($"Act: PATCH request to renew subscription {existing.Id} for {monthsToRenew} months", async () => {
            response = await client.PatchAsync($"{EndpointConst.Subscription}/{existing.Id}/renew?monthsToRenew={monthsToRenew}", null);
        });

        // Assert
        await AllureApi.Step("Assert: Validate response status and new DueDate", async () => {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<SubscriptionViewModel>(TestHelperBase.DefaultJsonOptions);
            result.ShouldNotBeNull();
            result.ShouldSatisfyAllConditions(
                () => result.Id.ShouldBe(existing.Id),
                () => result.DueDate.ShouldBe(expectedDate));
        });

        await AllureApi.Step("Assert: Verify DueDate update in Database", async () => {
            var dbEntity = await _dataSeedingHelper.FindEntityAsync<SubscriptionEntity>(existing.Id);
            dbEntity.ShouldNotBeNull();
            dbEntity.DueDate.ShouldBe(expectedDate);
        });

        await AllureApi.Step("Assert: Verify SubscriptionRenewedEvent was published to MassTransit", async () => {
            var wasPublished = await _harness.Published.Any<SubscriptionRenewedEvent>(x => x.Context.Message.Id == existing.Id);
            wasPublished.ShouldBeTrue("SubscriptionRenewedEvent was not published.");
        });
    }

    [Fact]
    [AllureSuite("Subscription API")]
    [AllureFeature("Information")]
    [AllureStory("Upcoming Bills")]
    [AllureDescription("Ensures the API returns only subscriptions due within the next 7 days")]
    public async Task GetUpcomingBills_WhenSubscriptionsAreDue_ShouldReturnOnlyItemsWithinSevenDays()
    {
        // Arrange
        HttpClient client = null!;
        DateOnly dueThreshold = default;

        await AllureApi.Step("Arrange: Seed mixed subscriptions (due and not due) and set 7-day threshold", async () => {
            client = _factory.CreateAuthenticatedClient();
            await _dataSeedingHelper.AddSeedUserWithUpcomingAndNonUpcomingSubscriptions();
            dueThreshold = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
        });

        // Act
        HttpResponseMessage response = null!;
        await AllureApi.Step("Act: GET request for upcoming user bills", async () => {
            response = await client.GetAsync($"{EndpointConst.Subscription}/bills/users");
        });

        // Assert
        await AllureApi.Step($"Assert: Verify all returned items have DueDate <= {dueThreshold}", async () => {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<List<SubscriptionViewModel>>(TestHelperBase.DefaultJsonOptions);
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
            result.ShouldAllBe(x => x.DueDate <= dueThreshold);
        });
    }
}
