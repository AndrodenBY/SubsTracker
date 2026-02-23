using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.DAL;
using SubsTracker.DAL.Entities;
using SubsTracker.IntegrationTests.Configuration;
using SubsTracker.IntegrationTests.DataSeedEntities;
using SubsTracker.IntegrationTests.Helpers;

namespace SubsTracker.IntegrationTests.User;

public class UserTestsDataSeedingHelper(TestsWebApplicationFactory factory) : TestHelperBase(factory)
{
    public async Task<UserSeedEntity> AddSeedUser()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var existingUsers = dbContext.Users
            .Where(u => u.Auth0Id == TestsAuthHandler.DefaultAuth0Id);
    
        if (existingUsers.Any())
        {
            dbContext.Users.RemoveRange(existingUsers);
            await dbContext.SaveChangesAsync(); 
        }
        
        var user = Fixture.Build<UserEntity>()
            .With(u => u.Auth0Id, TestsAuthHandler.DefaultAuth0Id) 
            .Without(u => u.Groups)
            .Without(u => u.Subscriptions)
            .Create();

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        return new UserSeedEntity
        {
            UserEntity = user,
            Subscriptions = new List<SubscriptionEntity>(),
            UserGroups = new List<GroupEntity>()
        };
    }

    public async Task<UserSeedEntity> AddSeedUserWithGroupsAndSubscriptions(string[] groupNames,
        string[] subscriptionNames)
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = Fixture.Build<UserEntity>()
            .Create();

        var groups = groupNames.Select(name =>
            Fixture.Build<GroupEntity>()
                .With(g => g.UserId, user.Id)
                .With(g => g.Name, name)
                .Create()
        ).ToList();

        var subscriptions = subscriptionNames.Select(name =>
            Fixture.Build<SubscriptionEntity>()
                .With(s => s.UserId, user.Id)
                .With(s => s.Name, name)
                .With(s => s.Active, true)
                .Without(s => s.User)
                .Create()
        ).ToList();

        await dbContext.Users.AddAsync(user);
        await dbContext.UserGroups.AddRangeAsync(groups);
        await dbContext.Subscriptions.AddRangeAsync(subscriptions);
        await dbContext.SaveChangesAsync();

        return new UserSeedEntity
        {
            UserEntity = user,
            Subscriptions = subscriptions,
            UserGroups = groups
        };
    }

    public CreateUserDto AddCreateUserDto()
    {
        var createDto = Fixture.Build<CreateUserDto>()
            .With(u => u.FirstName, "TestUser")
            .With(u => u.Email, "testuser@example.com")
            .Create();

        return createDto;
    }

    public UpdateUserDto AddUpdateUserDto(Guid userId)
    {
        var updateDto = Fixture.Build<UpdateUserDto>()
            .With(u => u.FirstName, "UpdatedName")
            .Create();

        return updateDto;
    }
}
