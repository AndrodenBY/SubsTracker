

namespace SubsTracker.IntegrationTests.Helpers;

public class SubscriptionTestHelper(TestsWebApplicationFactory factory) : TestHelperBase(factory)
{
    private User _user = null!;
    private List<Subscription> _subscriptions = null!;
    
    public async Task<Subscription> AddSingleSubscription()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        _user = _fixture.Build<User>()
            .Without(user => user.Groups)
            .Create();
        await dbContext.Users.AddAsync(_user);

        var subscription = _fixture.Build<Subscription>()
            .With(s => s.UserId, _user.Id)
            .Without(s => s.User)
            .Create();

        await dbContext.Subscriptions.AddAsync(subscription);
        await dbContext.SaveChangesAsync(default);

        var dbUser = dbContext.Users.FirstOrDefault(user => user.Id == _user.Id);
        var dbSubscription = dbContext.Subscriptions.FirstOrDefault(sub => sub.Id == subscription.Id);
        return subscription;
    }
    
    public async Task AddTestData()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        await ClearTestData();

        _user = _fixture.Create<User>();
        _subscriptions = Enumerable.Range(1, 5)
            .Select(_ => _fixture.Build<Subscription>()
                .With(s => s.UserId, _user.Id)
                .Create())
            .ToList();

        await dbContext.Users.AddAsync(_user);
        await dbContext.Subscriptions.AddRangeAsync(_subscriptions);
        await dbContext.SaveChangesAsync(default);
    }

    public async Task<CreateSubscriptionDto> AddCreateSubscriptionDtoInstance()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        
        _user = _fixture.Build<User>()
            .Without(user => user.Groups)
            .Create();
        
        await dbContext.Users.AddAsync(_user);
        await dbContext.SaveChangesAsync(default);
        
        var subscriptionDto = _fixture.Create<CreateSubscriptionDto>();
        return subscriptionDto;
    }

    public async Task<User> AddUserToDatabase()
    {
        var createdUser = _fixture.Build<User>()
            .Without(u => u.Groups)
            .Create();
        
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        
        await dbContext.Users.AddAsync(createdUser);
        await dbContext.SaveChangesAsync(default);

        dbContext.Users.FirstOrDefault(user => user.Id == createdUser.Id);
        return createdUser;
    }

    public async Task<Subscription> AddSubscription()
    {
        
        _user = _fixture.Build<User>()
            .Without(user => user.Groups)
            .Create();

        var createdSubscription = _fixture.Build<Subscription>()
            .With(s => s.UserId, _user.Id)
            .Without(s => s.User)
            .Create();
        
        return createdSubscription;
    }

    public async Task<Subscription> AddSubscriptionToDatabase()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        
        _user = _fixture.Build<User>()
            .Without(user => user.Groups)
            .Create();
        await dbContext.Users.AddAsync(_user);

        var createdSubscription = _fixture.Build<Subscription>()
            .With(s => s.UserId, _user.Id)
            .Without(s => s.User)
            .Create();
        
        
        await dbContext.Subscriptions.AddAsync(createdSubscription);
        await dbContext.SaveChangesAsync(default);

        dbContext.Subscriptions.FirstOrDefault(sub => sub.Id == createdSubscription.Id);
        return createdSubscription;
    }
    
    
    public async Task ClearTestData()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        dbContext.Users.RemoveRange(dbContext.Users.ToList());
        dbContext.Subscriptions.RemoveRange(dbContext.Subscriptions.ToList());
        await dbContext.SaveChangesAsync(default);
    }
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
