namespace SubsTracker.UnitTests.UserGroupService;

public class UserGroupServiceTestsBase
{
    private readonly IGroupMemberService _memberService;
    protected readonly ICacheService CacheService;
    protected readonly IFixture Fixture;
    protected readonly IUserGroupRepository GroupRepository;
    protected readonly IMapper Mapper;
    protected readonly GroupModelService Service;
    protected readonly ISubscriptionRepository SubscriptionRepository;
    protected readonly IRepository<User> UserRepository;

    protected UserGroupServiceTestsBase()
    {
        Fixture = new Fixture();
        Fixture.Customize<DateOnly>(composer => composer.FromFactory(() =>
            DateOnly.FromDateTime(DateTime.Today.AddDays(Fixture.Create<int>()))));
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        GroupRepository = Substitute.For<IUserGroupRepository>();
        UserRepository = Substitute.For<IRepository<User>>();
        SubscriptionRepository = Substitute.For<ISubscriptionRepository>();
        Mapper = Substitute.For<IMapper>();
        _memberService = Substitute.For<IGroupMemberService>();
        CacheService = Substitute.For<ICacheService>();

        Service = new GroupModelService(
            GroupRepository,
            UserRepository,
            SubscriptionRepository,
            _memberService,
            Mapper,
            CacheService
        );
    }
}
