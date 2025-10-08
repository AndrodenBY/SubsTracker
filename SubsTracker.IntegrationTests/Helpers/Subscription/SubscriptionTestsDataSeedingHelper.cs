namespace SubsTracker.IntegrationTests.Helpers.Subscription;

public class SubscriptionTestsDataSeedingHelper(TestsWebApplicationFactory factory) : TestHelperBase(factory)
{
    public async Task<SubscriptionSeedEntity> AddSeedData()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = _fixture.Build<UserModel>()
            .Without(u => u.Groups)
            .Create();

        var fixedDueDate = DateOnly.FromDateTime(DateTime.Today);

        var subscription = _fixture.Build<SubscriptionModel>()
            .With(s => s.UserId, user.Id)
            .With(s => s.Active, true)
            .With(s => s.DueDate, fixedDueDate)
            .Without(s => s.User)
            .Create();

        await dbContext.Users.AddAsync(user);
        await dbContext.Subscriptions.AddAsync(subscription);
        await dbContext.SaveChangesAsync(default);

        return new SubscriptionSeedEntity
        {
            User = user,
            Subscriptions = new List<SubscriptionModel> { subscription }
        };
    }

    public async Task<SubscriptionSeedEntity> AddSeedUserOnly()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = _fixture.Build<UserModel>()
            .Without(u => u.Groups)
            .Create();

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync(default);

        return new SubscriptionSeedEntity
        {
            User = user,
            Subscriptions = null!
        };
    }

    public async Task<SubscriptionSeedEntity> AddSeedUserWithSubscriptions(params string[] subscriptionNames)
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = _fixture.Build<UserModel>()
            .Without(u => u.Groups)
            .Create();

        var subscriptions = subscriptionNames.Select(name =>
            _fixture.Build<SubscriptionModel>()
                .With(s => s.UserId, user.Id)
                .With(s => s.Name, name)
                .With(s => s.Active, true)
                .Without(s => s.User)
                .Create()
        ).ToList();

        await dbContext.Users.AddAsync(user);
        await dbContext.Subscriptions.AddRangeAsync(subscriptions);
        await dbContext.SaveChangesAsync(default);

        return new SubscriptionSeedEntity
        {
            User = user,
            Subscriptions = subscriptions
        };
    }

    public CreateSubscriptionDto AddCreateSubscriptionDto()
    {
        var createSubscriptionDto = _fixture.Build<CreateSubscriptionDto>()
            .With(s => s.Content, SubscriptionContent.Design)
            .With(s => s.Type, SubscriptionType.Free)
            .Create();

        return createSubscriptionDto;
    }

    public UpdateSubscriptionDto AddUpdateSubscriptionDto(Guid updateTarget)
    {
        var updateSubscriptionDto = _fixture.Build<UpdateSubscriptionDto>()
            .With(s => s.Id, updateTarget)
            .Create();

        return updateSubscriptionDto;
    }

    public async Task<SubscriptionSeedEntity> AddSeedUserWithUpcomingAndNonUpcomingSubscriptions()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = _fixture.Build<UserModel>()
            .Without(u => u.Groups)
            .Create();

        var today = DateOnly.FromDateTime(DateTime.Today);

        var upcoming = _fixture.Build<SubscriptionModel>()
            .With(s => s.UserId, user.Id)
            .With(s => s.DueDate, today.AddDays(5))
            .With(s => s.Active, true)
            .Without(s => s.User)
            .Create();

        var distant = _fixture.Build<SubscriptionModel>()
            .With(s => s.UserId, user.Id)
            .With(s => s.DueDate, today.AddDays(20))
            .With(s => s.Active, true)
            .Without(s => s.User)
            .Create();

        var expired = _fixture.Build<SubscriptionModel>()
            .With(s => s.UserId, user.Id)
            .With(s => s.DueDate, today.AddDays(-2))
            .With(s => s.Active, true)
            .Without(s => s.User)
            .Create();

        await dbContext.Users.AddAsync(user);
        await dbContext.Subscriptions.AddRangeAsync(upcoming, distant, expired);
        await dbContext.SaveChangesAsync(default);

        return new SubscriptionSeedEntity
        {
            User = user,
            Subscriptions = new List<SubscriptionModel> { upcoming, distant, expired }
        };
    }
}
