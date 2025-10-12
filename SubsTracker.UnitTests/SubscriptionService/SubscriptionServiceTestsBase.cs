namespace SubsTracker.UnitTests.SubscriptionService;

public class SubscriptionServiceTestsBase
{
    protected readonly IFixture Fixture;
    protected readonly ISubscriptionRepository Repository;
    protected readonly IMapper Mapper;
    protected readonly IRepository<User> UserRepository;
    private readonly ISubscriptionHistoryRepository _historyRepository;
    protected readonly BLL.Services.Subscription.SubscriptionService Service;

    protected SubscriptionServiceTestsBase()
    {
        Fixture = new Fixture();
        Fixture.Customize<DateOnly>(composer => composer.FromFactory(() =>
            DateOnly.FromDateTime(DateTime.Today.AddDays(Fixture.Create<int>()))));
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        Repository = Substitute.For<ISubscriptionRepository>();
        UserRepository = Substitute.For<IRepository<User>>();
        Mapper = Substitute.For<IMapper>();
        _historyRepository = Substitute.For<ISubscriptionHistoryRepository>();
        Service = new BLL.Services.Subscription.SubscriptionService(Repository, Mapper, UserRepository, _historyRepository);
    }
}
