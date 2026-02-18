using DispatchR;

namespace SubsTracker.UnitTests.GroupMemberService;

public class GroupMemberServiceTestBase
{
    protected readonly ICacheService CacheService;
    protected readonly IFixture Fixture;
    protected readonly IMapper Mapper;
    protected readonly IGroupMemberRepository MemberRepository;
    protected readonly IMediator Mediator;
    protected readonly MemberModelService Service;

    protected GroupMemberServiceTestBase()
    {
        Fixture = new Fixture();
        Fixture.Customize<DateOnly>(composer => composer.FromFactory(() =>
            DateOnly.FromDateTime(DateTime.Today.AddDays(Fixture.Create<int>()))));
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        MemberRepository = Substitute.For<IGroupMemberRepository>();
        Mediator = Substitute.For<IMediator>();
        Mapper = Substitute.For<IMapper>();
        CacheService = Substitute.For<ICacheService>();

        Service = new MemberModelService(
            MemberRepository,
            Mapper,
            CacheService,
            Mediator
        );
    }
}
