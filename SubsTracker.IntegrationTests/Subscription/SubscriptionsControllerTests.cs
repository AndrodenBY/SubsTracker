using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SubsTracker.API.ViewModel;
using SubsTracker.DAL;
using SubsTracker.IntegrationTests.Configuration;
using SubsTracker.IntegrationTests.Constants;
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
        
        var result = await response.Content.ReadFromJsonAsync<SubscriptionViewModel>();
        result.ShouldNotBeNull();
        result.ShouldSatisfyAllConditions(
            () => result.ShouldNotBeNull(),
            () => result.Id.ShouldBe(expected.Id),
            () => result.Name.ShouldBe(expected.Name),
            () => result.Price.ShouldBe(expected.Price),
            () => result.Type.ToString().ShouldBe(expected.Type.ToString())
        );
    }

    [Fact]
    public async Task GetAll_WhenFilteredByName_ReturnsOnlyMatchingSubscription()
    {
        //Arrange
        const string targetName = "Target Subscription";
        await _dataSeedingHelper.AddSeedUserWithSubscriptions(targetName, "Unrelated App");

        //Act
        var response = await _client.GetAsync($"{EndpointConst.Subscription}?Name={targetName}");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<SubscriptionViewModel>>();

        result.ShouldNotBeNull();
        result.ShouldHaveSingleItem()
            .Name.ShouldBe(targetName);
        result.ShouldNotContain(x => x.Name == "Unrelated App");
    }

    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentName_ReturnsEmptyList()
    {
        //Arrange
        await _dataSeedingHelper.AddSeedData();
        const string nonExistentName = "NonExistentFilter";

        //Act
        var response = await _client.GetAsync($"{EndpointConst.Subscription}?Name={nonExistentName}");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<SubscriptionViewModel>>();
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
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
        
        var result = await response.Content.ReadFromJsonAsync<SubscriptionViewModel>();
        result.ShouldNotBeNull();
        result.ShouldSatisfyAllConditions(
            () => result.Id.ShouldNotBe(Guid.Empty),
            () => result.Name.ShouldBe(dto.Name),
            () => result.Price.ShouldBe(dto.Price),
            () => result.DueDate.ShouldBe(dto.DueDate)
        );
        
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        var entity = await db.Subscriptions.FindAsync(result.Id);

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
        
        var result = await response.Content.ReadFromJsonAsync<SubscriptionViewModel>();
        result.ShouldNotBeNull();
        result.ShouldSatisfyAllConditions(
            () => result.Id.ShouldBe(existing.Id),
            () => result.Name.ShouldBe(updateDto.Name),
            () => result.Price.ShouldBe((decimal)updateDto.Price!)
        );
        
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        
        var dbEntity = await db.Subscriptions.FindAsync(existing.Id);
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
    
        var result = await response.Content.ReadFromJsonAsync<SubscriptionViewModel>();
        result.ShouldNotBeNull();
        result.Id.ShouldBe(expected.Id);
        
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        var entity = await db.Subscriptions.FindAsync(expected.Id);
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

        //Act
        var response = await _client.PatchAsync($"{EndpointConst.Subscription}/{existing.Id}/renew?monthsToRenew={monthsToRenew}", null);

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<SubscriptionViewModel>();
        result.ShouldNotBeNull();
        result.ShouldSatisfyAllConditions(
            () => result.Id.ShouldBe(existing.Id),
            () => result.DueDate.ShouldBe(expectedDate));
        
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        var dbEntity = await db.Subscriptions.FindAsync(existing.Id);
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
        
        var dueBill = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

        //Act
        var response = await client.GetAsync($"{EndpointConst.Subscription}/bills/users");
    
        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<List<SubscriptionViewModel>>();
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty("The test expected upcoming bills, but the list was empty.");
        result.ShouldSatisfyAllConditions(
            () => result.ShouldAllBe(x => x.DueDate <= dueBill),
            () => result.Any(x => x.DueDate <= dueBill).ShouldBeTrue());
    }
}
