namespace SubsTracker.UnitTests.UserGroupService;

public class UserGroupServiceTestsBase
{
    protected readonly IFixture Fixture;
    protected readonly IUserGroupRepository GroupRepository;
    protected readonly IRepository<User> UserRepository;
    protected readonly ISubscriptionRepository SubscriptionRepository;
    private readonly IGroupMemberService _memberService;
    protected readonly IMapper Mapper;
    protected readonly BLL.Services.User.UserGroupService Service;

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

        Service = new BLL.Services.User.UserGroupService(
            GroupRepository,
            UserRepository,
            SubscriptionRepository,
            _memberService,
            Mapper
       );
    }
}
