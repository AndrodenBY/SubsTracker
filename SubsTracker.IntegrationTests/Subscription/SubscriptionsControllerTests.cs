using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SubsTracker.API.ViewModel;
using SubsTracker.DAL;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Pagination;
using SubsTracker.IntegrationTests.Configuration;
using SubsTracker.IntegrationTests.Constants;
using SubsTracker.IntegrationTests.Helpers;
using SubsTracker.Messaging.Contracts;

namespace SubsTracker.IntegrationTests.Subscription;

public class SubscriptionsControllerTests : IClassFixture<TestsWebApplicationFactory>
{
    private readonly TestsWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly SubscriptionTestsDataSeedingHelper _dataSeedingHelper;
    private readonly ITestHarness _harness;

    public SubscriptionsControllerTests(TestsWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestAuthScheme");
        _dataSeedingHelper = new SubscriptionTestsDataSeedingHelper(factory);
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
        var expected = seed.Subscriptions.First();
    
        var client = _factory.CreateAuthenticatedClient();

        //Act
        var response = await client.GetAsync($"{EndpointConst.Subscription}/{expected.Id}");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<SubscriptionViewModel>(TestHelperBase.DefaultJsonOptions);
        result.ShouldNotBeNull();
        result.ShouldSatisfyAllConditions(
            () => result.Id.ShouldBe(expected.Id),
            () => result.Name.ShouldBe(expected.Name),
            () => result.Price.ShouldBe(expected.Price),
            () => result.Type.ShouldBe(expected.Type)
        );
    }

    [Fact]
    public async Task GetAll_WhenFilteredByName_ReturnsOnlyMatchingSubscription()
    {
        //Arrange
        const string targetName = "Target Subscription";
        await _dataSeedingHelper.AddSeedUserWithSubscriptions(targetName, "Unrelated App");
        var client = _factory.CreateAuthenticatedClient();

        //Act
        var response = await client.GetAsync($"{EndpointConst.Subscription}?Name={targetName}");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<PaginatedList<SubscriptionViewModel>>(TestHelperBase.DefaultJsonOptions);
        result.ShouldNotBeNull();
        result.Items.ShouldHaveSingleItem();
        result.Items[0].Name.ShouldBe(targetName);
        result.Items.ShouldNotContain(x => x.Name == "Unrelated App");
    }

    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentName_ReturnsEmptyList()
    {
        //Arrange
        await _dataSeedingHelper.AddSeedData();
        const string nonExistentName = "NonExistentFilter";
        var client = _factory.CreateAuthenticatedClient();

        //Act
        var response = await client.GetAsync($"{EndpointConst.Subscription}?Name={nonExistentName}");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedList<SubscriptionViewModel>>(TestHelperBase.DefaultJsonOptions);
        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Create_WhenValidData_ReturnsCreatedSubscription()
    {
        //Arrange
        var dto = _dataSeedingHelper.AddCreateSubscriptionDto();
        await _dataSeedingHelper.AddSeedUserOnly();
        var client = _factory.CreateAuthenticatedClient();

        //Act
        var response = await client.PostAsJsonAsync(EndpointConst.Subscription, dto);

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK); 
        
        var result = await response.Content.ReadFromJsonAsync<SubscriptionViewModel>(TestHelperBase.DefaultJsonOptions);
        result.ShouldNotBeNull();
        result.ShouldSatisfyAllConditions(
            () => result.Id.ShouldNotBe(Guid.Empty),
            () => result.Name.ShouldBe(dto.Name),
            () => result.Price.ShouldBe(dto.Price),
            () => result.DueDate.ShouldBe(dto.DueDate)
        );
        
        var entity = await _dataSeedingHelper.FindEntityAsync<SubscriptionEntity>(result.Id);
        entity.ShouldNotBeNull();
        entity.Name.ShouldBe(dto.Name);
    }

    [Fact]
    public async Task Update_WhenValidData_ReturnsUpdatedSubscription()
    {
        //Arrange
        var seed = await _dataSeedingHelper.AddSeedData();
        var existing = seed.Subscriptions.First();
        var updateDto = _dataSeedingHelper.AddUpdateSubscriptionDto(existing.Id);
        var client = _factory.CreateAuthenticatedClient();

        //Act
        var response = await client.PutAsJsonAsync(EndpointConst.Subscription, updateDto);

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<SubscriptionViewModel>(TestHelperBase.DefaultJsonOptions);
        result.ShouldNotBeNull();
        result.ShouldSatisfyAllConditions(
            () => result.Id.ShouldBe(existing.Id),
            () => result.Name.ShouldBe(updateDto.Name),
            () => result.Price.ShouldBe((decimal)updateDto.Price!)
        );
        
        var dbEntity = await _dataSeedingHelper.FindEntityAsync<SubscriptionEntity>(existing.Id);
        dbEntity.ShouldNotBeNull();
        dbEntity.Name.ShouldBe(updateDto.Name);
        dbEntity.Price.ShouldBe((decimal)updateDto.Price!);
    }
    
    [Fact]
    public async Task CancelSubscription_ShouldUpdateDatabaseAndPublishEvent()
    {
        //Arrange
        var seed = await _dataSeedingHelper.AddSeedUserWithSubscriptions("Streaming Service");
        var expected = seed.Subscriptions.First();
        var client = _factory.CreateAuthenticatedClient();

        //Act
        var response = await client.PatchAsync($"{EndpointConst.Subscription}/{expected.Id}/cancel", null);

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<SubscriptionViewModel>(TestHelperBase.DefaultJsonOptions);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(expected.Id);
        
        var entity = await _dataSeedingHelper.FindEntityAsync<SubscriptionEntity>(expected.Id);
        entity.ShouldNotBeNull();
        entity.Active.ShouldBeFalse();
        
        var wasPublished = await _harness.Published.Any<SubscriptionCanceledEvent>(x => x.Context.Message.Id == expected.Id);
        wasPublished.ShouldBeTrue("The SubscriptionCanceledEvent was not published to the bus.");
    }
    
    [Fact]
    public async Task RenewSubscription_ShouldUpdateDueDateAndPublishEvent()
    {
        //Arrange
        var seed = await _dataSeedingHelper.AddSeedData();
        var existing = seed.Subscriptions.First();
        const int monthsToRenew = 3;
        var expectedDate = existing.DueDate.AddMonths(monthsToRenew);
        var client = _factory.CreateAuthenticatedClient();

        //Act
        var response = await client.PatchAsync($"{EndpointConst.Subscription}/{existing.Id}/renew?monthsToRenew={monthsToRenew}", null);

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<SubscriptionViewModel>(TestHelperBase.DefaultJsonOptions);
        result.ShouldNotBeNull();
        result.ShouldSatisfyAllConditions(
            () => result.Id.ShouldBe(existing.Id),
            () => result.DueDate.ShouldBe(expectedDate));
        
        var dbEntity = await _dataSeedingHelper.FindEntityAsync<SubscriptionEntity>(existing.Id);
        dbEntity.ShouldNotBeNull();
        dbEntity.DueDate.ShouldBe(expectedDate);
        
        var wasPublished = await _harness.Published.Any<SubscriptionRenewedEvent>(x => x.Context.Message.Id == existing.Id);
        wasPublished.ShouldBeTrue("SubscriptionRenewedEvent was not published.");
    }

    [Fact]
    public async Task GetUpcomingBills_WhenSubscriptionsAreDue_ShouldReturnOnlyItemsWithinSevenDays()
    {
        //Arrange
        var client = _factory.CreateAuthenticatedClient();
        await _dataSeedingHelper.AddSeedUserWithUpcomingAndNonUpcomingSubscriptions();
        
        var dueThreshold = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

        //Act
        var response = await client.GetAsync($"{EndpointConst.Subscription}/bills/users");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    
        var result = await response.Content.ReadFromJsonAsync<List<SubscriptionViewModel>>(TestHelperBase.DefaultJsonOptions);
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldAllBe(x => x.DueDate <= dueThreshold);
    }
}
