namespace SubsTracker.UnitTests.UserGroupService;

public class UserGroupServiceTestsBase
{
    protected readonly IFixture _fixture;
    protected readonly IRepository<UserGroup> _repository;
    protected readonly IRepository<User> _userRepository;
    protected readonly ISubscriptionRepository _subscriptionRepository;
    protected readonly IGroupMemberService _memberService;
    protected readonly IMapper _mapper;
    protected readonly BLL.Services.User.UserGroupService _service;

    protected UserGroupServiceTestsBase()
    {
        _fixture = new Fixture();
        _fixture.Customize<DateOnly>(composer => composer.FromFactory(() =>
            DateOnly.FromDateTime(DateTime.Today.AddDays(_fixture.Create<int>()))));
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        
        _repository = Substitute.For<IRepository<UserGroup>>();
        _userRepository = Substitute.For<IRepository<User>>();
        _subscriptionRepository = Substitute.For<ISubscriptionRepository>();
        _mapper = Substitute.For<IMapper>();
        _memberService = Substitute.For<IGroupMemberService>();
        
        _service = new BLL.Services.User.UserGroupService(
            _repository, 
            _userRepository, 
            _subscriptionRepository, 
            _memberService, 
            _mapper
        );
    }
}
