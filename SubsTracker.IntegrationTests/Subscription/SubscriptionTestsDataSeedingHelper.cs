using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.DAL;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Enums;
using SubsTracker.IntegrationTests.Configuration;
using SubsTracker.IntegrationTests.DataSeedEntities;
using SubsTracker.IntegrationTests.Helpers;

namespace SubsTracker.IntegrationTests.Subscription;

public class SubscriptionTestsDataSeedingHelper(TestsWebApplicationFactory factory) : TestHelperBase(factory)
{
    
    public async Task<SubscriptionSeedEntity> AddSeedData()
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        
        var existingUser = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Auth0Id == TestsAuthHandler.DefaultAuth0Id);
        if (existingUser != null)
        {
            dbContext.Users.Remove(existingUser);
            await dbContext.SaveChangesAsync();
        }

        var user = Fixture.Build<UserEntity>()
            .With(u => u.Auth0Id, TestsAuthHandler.DefaultAuth0Id)
            .Without(u => u.Groups)
            .Without(u => u.Subscriptions)
            .Create();

        var subscription = Fixture.Build<SubscriptionEntity>()
            .With(s => s.UserId, user.Id)
            .Without(s => s.User)
            .Without(s => s.History) 
            .Create();

        await dbContext.Users.AddAsync(user);
        await dbContext.Subscriptions.AddAsync(subscription);
        await dbContext.SaveChangesAsync();

        return new SubscriptionSeedEntity 
        { 
            UserEntity = user,
            Subscriptions = [subscription]
        };
    }

    public async Task<SubscriptionSeedEntity> AddSeedUserOnly()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = Fixture.Build<UserEntity>()
            .With(u => u.Auth0Id, TestsAuthHandler.DefaultAuth0Id)
            .Without(u => u.Groups)
            .Create();

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        return new SubscriptionSeedEntity
        {
            UserEntity = user,
            Subscriptions = null!
        };
    }

    public async Task<SubscriptionSeedEntity> AddSeedUserWithSubscriptions(params string[] subscriptionNames)
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = Fixture.Build<UserEntity>()
            .With(u => u.Auth0Id, TestsAuthHandler.DefaultAuth0Id)
            .Without(u => u.Groups)
            .Create();

        var subscriptions = subscriptionNames.Select(name =>
            Fixture.Build<SubscriptionEntity>()
                .With(s => s.UserId, user.Id)
                .With(s => s.Name, name)
                .With(s => s.Active, true)
                .Without(s => s.User)
                .Create()
        ).ToList();

        await dbContext.Users.AddAsync(user);
        await dbContext.Subscriptions.AddRangeAsync(subscriptions);
        await dbContext.SaveChangesAsync();

        return new SubscriptionSeedEntity
        {
            UserEntity = user,
            Subscriptions = subscriptions
        };
    }

    public CreateSubscriptionDto AddCreateSubscriptionDto()
    {
        var createSubscriptionDto = Fixture.Build<CreateSubscriptionDto>()
            .With(s => s.Content, SubscriptionContent.Design)
            .With(s => s.Type, SubscriptionType.Free)
            .Create();

        return createSubscriptionDto;
    }

    public UpdateSubscriptionDto AddUpdateSubscriptionDto(Guid updateTarget)
    {
        var updateSubscriptionDto = Fixture.Build<UpdateSubscriptionDto>()
            .With(s => s.Id, updateTarget)
            .Create();

        return updateSubscriptionDto;
    }

    public async Task<SubscriptionSeedEntity> AddSeedUserWithUpcomingAndNonUpcomingSubscriptions()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = Fixture.Build<UserEntity>()
            .With(u => u.Auth0Id, TestsAuthHandler.DefaultAuth0Id)
            .Without(u => u.Groups)
            .Create();

        var today = DateOnly.FromDateTime(DateTime.Today);

        var upcoming = Fixture.Build<SubscriptionEntity>()
            .With(s => s.UserId, user.Id)
            .With(s => s.DueDate, today.AddDays(5))
            .With(s => s.Active, true)
            .Without(s => s.User)
            .Create();

        var distant = Fixture.Build<SubscriptionEntity>()
            .With(s => s.UserId, user.Id)
            .With(s => s.DueDate, today.AddDays(20))
            .With(s => s.Active, true)
            .Without(s => s.User)
            .Create();

        var expired = Fixture.Build<SubscriptionEntity>()
            .With(s => s.UserId, user.Id)
            .With(s => s.DueDate, today.AddDays(-2))
            .With(s => s.Active, true)
            .Without(s => s.User)
            .Create();

        await dbContext.Users.AddAsync(user);
        await dbContext.Subscriptions.AddRangeAsync(upcoming, distant, expired);
        await dbContext.SaveChangesAsync();

        return new SubscriptionSeedEntity
        {
            UserEntity = user,
            Subscriptions = [upcoming, distant, expired]
        };
    }
}
