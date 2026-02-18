using DispatchR;

namespace SubsTracker.UnitTests.SubscriptionService;

public class SubscriptionServiceTestsBase
{
    protected readonly ICacheService CacheService;
    protected readonly IFixture Fixture;
    protected readonly IMapper Mapper;
    protected readonly IMediator Mediator;
    protected readonly SubscriptionModelService Service;
    protected readonly ISubscriptionRepository SubscriptionRepository;
    protected readonly IUserRepository UserRepository;

    protected SubscriptionServiceTestsBase()
    {
        Fixture = new Fixture();
        Fixture.Customize<DateOnly>(composer => composer.FromFactory(() =>
            DateOnly.FromDateTime(DateTime.Today.AddDays(Fixture.Create<int>()))));
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        SubscriptionRepository = Substitute.For<ISubscriptionRepository>();
        UserRepository = Substitute.For<IUserRepository>();
        Mapper = Substitute.For<IMapper>();
        Mediator = Substitute.For<IMediator>();
        CacheService = Substitute.For<ICacheService>();

        Service = new SubscriptionModelService(
            SubscriptionRepository,
            Mapper,
            UserRepository,
            CacheService,
            Mediator
        );
    }
}
