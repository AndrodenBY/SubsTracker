

namespace SubsTracker.IntegrationTests.Helpers;

public class SubscriptionTestHelper(TestsWebApplicationFactory factory) : TestHelperBase(factory)
{
    public async Task<SubscriptionSeedEntity> AddSeedData()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = _fixture.Build<User>()
            .Without(u => u.Groups)
            .Create();

        var fixedDueDate = DateOnly.FromDateTime(DateTime.Today);

        var subscription = _fixture.Build<DAL.Models.Subscription.Subscription>()
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
            Subscriptions = new List<DAL.Models.Subscription.Subscription> { subscription }
        };
    }

    public async Task<SubscriptionSeedEntity> AddSeedUserOnly()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = _fixture.Build<User>()
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

        var user = _fixture.Build<User>()
            .Without(u => u.Groups)
            .Create();

        var subscriptions = subscriptionNames.Select(name =>
            _fixture.Build<DAL.Models.Subscription.Subscription>()
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

    public async Task<CreateSubscriptionDto> AddCreateSubscriptionDto()
    {
        var createSubscriptionDto = _fixture.Build<CreateSubscriptionDto>()
            .With(s => s.Content, SubscriptionContent.Design)
            .With(s => s.Type, SubscriptionType.Free)
            .Create();
        
        return createSubscriptionDto;
    }

    public async Task<UpdateSubscriptionDto> AddUpdateSubscriptionDto(Guid updateTarget)
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

        var user = _fixture.Build<User>()
            .Without(u => u.Groups)
            .Create();

        var today = DateOnly.FromDateTime(DateTime.Today);

        var upcoming = _fixture.Build<DAL.Models.Subscription.Subscription>()
            .With(s => s.UserId, user.Id)
            .With(s => s.DueDate, today.AddDays(3)) // попадает в окно 7 дней
            .With(s => s.Active, true)
            .Without(s => s.User)
            .Create();

        var distant = _fixture.Build<DAL.Models.Subscription.Subscription>()
            .With(s => s.UserId, user.Id)
            .With(s => s.DueDate, today.AddDays(15)) // слишком далеко
            .With(s => s.Active, true)
            .Without(s => s.User)
            .Create();

        var expired = _fixture.Build<DAL.Models.Subscription.Subscription>()
            .With(s => s.UserId, user.Id)
            .With(s => s.DueDate, today.AddDays(-2)) // уже просрочена
            .With(s => s.Active, true)
            .Without(s => s.User)
            .Create();

        await dbContext.Users.AddAsync(user);
        await dbContext.Subscriptions.AddRangeAsync(upcoming, distant, expired);
        await dbContext.SaveChangesAsync(default);

        return new SubscriptionSeedEntity
        {
            User = user,
            Subscriptions = new List<DAL.Models.Subscription.Subscription> { upcoming, distant, expired }
        };
    }

    public async Task ClearTestData()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        dbContext.Users.RemoveRange(dbContext.Users.ToList());
        dbContext.Subscriptions.RemoveRange(dbContext.Subscriptions.ToList());
        await dbContext.SaveChangesAsync(default);
    }
    
    public async Task ClearTestDataWithRelations()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        
        dbContext.UserGroups.RemoveRange(dbContext.UserGroups.ToList());
        dbContext.Subscriptions.RemoveRange(dbContext.Subscriptions.ToList());
        dbContext.Users.RemoveRange(dbContext.Users.ToList());

        await dbContext.SaveChangesAsync(default);
    }



// private readonly IServiceScopeFactory _scopeFactory;
//
// public SubscriptionTestHelper(IServiceScopeFactory scopeFactory)
// {
//     _scopeFactory = scopeFactory;
// }
//
// public void AddTestData(User user, Subscription subscription)
// {
//     using var scope = _scopeFactory.CreateScope();
//     var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
//     
//     // Очистка данных перед добавлением (для изоляции тестов)
//     dbContext.Users.RemoveRange(dbContext.Users.ToList());
//     dbContext.Subscriptions.RemoveRange(dbContext.Subscriptions.ToList());
//
//     dbContext.Users.Add(user);
//     dbContext.Subscriptions.Add(subscription);
//     
//     dbContext.SaveChanges();
// }
//
// public CreateSubscriptionDto CreateSubscriptionDto(string name, decimal price)
// {
//     return new CreateSubscriptionDto
//     {
//         Name = name,
//         Price = price,
//         DueDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
//         Type = Domain.Enums.SubscriptionType.Standard,
//         Content = Domain.Enums.SubscriptionContent.Entertainment
//     };
// }
//
// public void ClearTestData()
// {
//     using var scope = _scopeFactory.CreateScope();
//     var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
//     
//     dbContext.Users.RemoveRange(dbContext.Users.ToList());
//     dbContext.Subscriptions.RemoveRange(dbContext.Subscriptions.ToList());
//     
//     dbContext.SaveChanges();
// }
