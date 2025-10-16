namespace SubsTracker.UnitTests.GroupMemberService;

public class GroupMemberServiceTestBase
{
    protected readonly IFixture Fixture;
    protected readonly IGroupMemberRepository MemberRepository;
    //protected readonly IMessageService MessageService;
    protected readonly IMapper Mapper;
    protected readonly BLL.Services.User.GroupMemberService Service;

    protected GroupMemberServiceTestBase()
    {
        Fixture = new Fixture();
        Fixture.Customize<DateOnly>(composer => composer.FromFactory(() =>
             DateOnly.FromDateTime(DateTime.Today.AddDays(Fixture.Create<int>()))));
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
             .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        MemberRepository = Substitute.For<IGroupMemberRepository>();
        //MessageService = Substitute.For<IMessageService>();
        Mapper = Substitute.For<IMapper>();

        Service = new BLL.Services.User.GroupMemberService(
            MemberRepository,
            //MessageService,
            Mapper
       );
    }
}
