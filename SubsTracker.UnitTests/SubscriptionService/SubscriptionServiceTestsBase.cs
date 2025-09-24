namespace SubsTracker.UnitTests.SubscriptionService;

public class SubscriptionServiceTestsBase
{
    protected readonly IFixture _fixture;
    protected readonly ISubscriptionRepository _repository;
    protected readonly IMapper _mapper;
    protected readonly IRepository<User> _userRepository;
    private ISubscriptionHistoryRepository _history;
    protected readonly BLL.Services.Subscription.SubscriptionService _service;

    protected SubscriptionServiceTestsBase()
    {
        _fixture = new Fixture();
        _fixture.Customize<DateOnly>(composer => composer.FromFactory(() =>
            DateOnly.FromDateTime(DateTime.Today.AddDays(_fixture.Create<int>()))));
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        
        _repository = Substitute.For<ISubscriptionRepository>();
        _userRepository = Substitute.For<IRepository<User>>();
        _mapper = Substitute.For<IMapper>();
        _history = Substitute.For<ISubscriptionHistoryRepository>();
        _service = new BLL.Services.Subscription.SubscriptionService(_repository, _mapper, _userRepository, _history);
    }
}
