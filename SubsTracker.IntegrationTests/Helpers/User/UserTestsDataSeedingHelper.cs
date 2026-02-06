namespace SubsTracker.IntegrationTests.Helpers.User;

public class UserTestsDataSeedingHelper(TestsWebApplicationFactory factory) : TestHelperBase(factory)
{
    public async Task<UserSeedEntity> AddSeedUser()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = Fixture.Build<UserModel>()
            .Without(u => u.Groups)
            .Without(u => u.Subscriptions)
            .Create();

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        return new UserSeedEntity
        {
            User = user,
            Subscriptions = new List<SubscriptionModel>(),
            UserGroups = new List<Group>()
        };
    }

    public async Task<UserSeedEntity> AddSeedUserWithGroupsAndSubscriptions(string[] groupNames,
        string[] subscriptionNames)
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = Fixture.Build<UserModel>()
            .Create();

        var groups = groupNames.Select(name =>
            Fixture.Build<Group>()
                .With(g => g.UserId, user.Id)
                .With(g => g.Name, name)
                .Create()
        ).ToList();

        var subscriptions = subscriptionNames.Select(name =>
            Fixture.Build<SubscriptionModel>()
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
            User = user,
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
            .With(u => u.Email, "updated@example.com")
            .Create();

        return updateDto;
    }
}
