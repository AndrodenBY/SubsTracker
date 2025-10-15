namespace SubsTracker.UnitTests.SubscriptionService;

public class SubscriptionServiceTestsBase
{
    protected readonly IFixture Fixture;
    protected readonly ISubscriptionRepository SubscriptionRepository;
    protected readonly IRepository<Subscription> GenericRepository;
    protected readonly IMapper Mapper;
    protected readonly IRepository<User> UserRepository;
    //protected readonly IMessageService MessageService;
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

        SubscriptionRepository = Substitute.For<ISubscriptionRepository>();
        GenericRepository = Substitute.For<IRepository<Subscription>>();
        UserRepository = Substitute.For<IRepository<User>>();
        //MessageService = Substitute.For<IMessageService>();
        Mapper = Substitute.For<IMapper>();
        _historyRepository = Substitute.For<ISubscriptionHistoryRepository>();
        Service = new BLL.Services.Subscription.SubscriptionService(
            SubscriptionRepository, 
            GenericRepository, 
            //MessageService, 
            Mapper, 
            UserRepository, 
            _historyRepository
        );
    }
}
