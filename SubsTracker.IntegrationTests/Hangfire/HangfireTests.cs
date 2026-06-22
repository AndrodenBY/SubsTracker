using Allure.Net.Commons;
using Allure.Xunit.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SubsTracker.BLL.Interfaces;
using SubsTracker.DAL;
using SubsTracker.IntegrationTests.Configuration;
using SubsTracker.IntegrationTests.DataSeedEntities;
using SubsTracker.IntegrationTests.Subscription;

namespace SubsTracker.IntegrationTests.Hangfire;

public class HangfireTests : IClassFixture<TestsWebApplicationFactory>
{
    private readonly TestsWebApplicationFactory _factory;
    private readonly SubscriptionTestsDataSeedingHelper _dataSeedingHelper;

    public HangfireTests(TestsWebApplicationFactory factory)
    {
        _factory = factory;
        _dataSeedingHelper = new SubscriptionTestsDataSeedingHelper(factory);
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }
    
    [Fact]
    [AllureSuite("Subscription API")]
    [AllureFeature("Background Jobs")]
    [AllureStory("Process Expired Subscriptions")]
    [AllureDescription("Verifies that expired subscriptions are automatically deactivated")]
    public async Task ProcessExpiredSubscriptions_ShouldDeactivateExpiredSubscriptions()
    {
        // Arrange
        SubscriptionSeedEntity seed = null!;

        await AllureApi.Step("Arrange: Seed expired subscriptions", async () =>
        {
            seed = await _dataSeedingHelper.AddSeedUserWithExpiredSubscriptions();
        });

        // Act
        await AllureApi.Step("Act: Execute expiration job", async () =>
        {
            using var scope = _factory.Services.CreateScope();

            var service = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();

            await service.ProcessExpiredSubscriptions(CancellationToken.None);
        });

        // Assert
        await AllureApi.Step("Assert: Expired subscriptions should be inactive", async () =>
        {
            using var scope = _factory.Services.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

            var updatedSubscriptions = await dbContext.Subscriptions
                .Where(s => seed.Subscriptions.Select(x => x.Id).Contains(s.Id))
                .ToListAsync();

            updatedSubscriptions.ShouldAllBe(s => s.Active == false);
        });
    }
}
