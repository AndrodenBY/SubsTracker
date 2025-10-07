using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;

namespace SubsTracker.IntegrationTests.Helpers.User;

public class UserTestsDataSeedingHelper(TestsWebApplicationFactory factory) : TestHelperBase(factory)
{
    public async Task<UserSeedEntity> AddSeedUser()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = _fixture.Build<UserModel>()
            .Without(u => u.Groups)
            .Without(u => u.Subscriptions)
            .Create();

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync(default);

        return new UserSeedEntity
        {
            User = user,
            Subscriptions = new(),
            UserGroups = new()
        };
    }
    
    public async Task<UserSeedEntity> AddSeedUserWithGroupsAndSubscriptions(string[] groupNames, string[] subscriptionNames)
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = _fixture.Build<UserModel>()
            .Create();

        var groups = groupNames.Select(name =>
            _fixture.Build<Group>()
                .With(g => g.UserId, user.Id)
                .With(g => g.Name, name)
                .Create()
        ).ToList();

        var subscriptions = subscriptionNames.Select(name =>
            _fixture.Build<SubscriptionModel>()
                .With(s => s.UserId, user.Id)
                .With(s => s.Name, name)
                .With(s => s.Active, true)
                .Without(s => s.User)
                .Create()
        ).ToList();

        await dbContext.Users.AddAsync(user);
        await dbContext.UserGroups.AddRangeAsync(groups);
        await dbContext.Subscriptions.AddRangeAsync(subscriptions);
        await dbContext.SaveChangesAsync(default);

        return new UserSeedEntity
        {
            User = user,
            Subscriptions = subscriptions,
            UserGroups = groups
        };
    }
    
    public async Task<CreateUserDto> AddCreateUserDto()
    {
        var createDto = _fixture.Build<CreateUserDto>()
            .With(u => u.FirstName, "TestUser")
            .With(u => u.Email, "testuser@example.com")
            .Create();

        return createDto;
    }

    public async Task<UpdateUserDto> AddUpdateUserDto(Guid userId)
    {
        var updateDto = _fixture.Build<UpdateUserDto>()
            .With(u => u.Id, userId)
            .With(u => u.FirstName, "UpdatedName")
            .With(u => u.Email, "updated@example.com")
            .Create();

        return updateDto;
    }


    public async Task ClearTestDataWithDependencies()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        dbContext.UserGroups.RemoveRange(dbContext.UserGroups.ToList());
        dbContext.Subscriptions.RemoveRange(dbContext.Subscriptions.ToList());
        dbContext.Users.RemoveRange(dbContext.Users.ToList());

        await dbContext.SaveChangesAsync(default);
    }
}
